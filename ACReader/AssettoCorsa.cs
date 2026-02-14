using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning; // ✅ добавлено
using System.Text;
using System.Threading.Tasks;

namespace AssettoCorsaSharedMemory
{
    public delegate void PhysicsUpdatedHandler(object? sender, PhysicsEventArgs e);
    public delegate void GraphicsUpdatedHandler(object? sender, GraphicsEventArgs e);
    public delegate void StaticInfoUpdatedHandler(object? sender, StaticInfoEventArgs e);
    public delegate void GameStatusChangedHandler(object? sender, GameStatusEventArgs e);

    public class AssettoCorsaNotStartedException : Exception
    {
        public AssettoCorsaNotStartedException()
            : base("Shared Memory not connected, is Assetto Corsa running and have you run assettoCorsa.Start()?")
        {
        }
    }

    enum AC_MEMORY_STATUS { DISCONNECTED, CONNECTING, CONNECTED }

    public class AssettoCorsa
    {
        private System.Timers.Timer sharedMemoryRetryTimer;
        private AC_MEMORY_STATUS memoryStatus = AC_MEMORY_STATUS.DISCONNECTED;
        public bool IsRunning => memoryStatus == AC_MEMORY_STATUS.CONNECTED;

        private AC_STATUS gameStatus = AC_STATUS.AC_OFF;

        public event GameStatusChangedHandler? GameStatusChanged;
        public virtual void OnGameStatusChanged(GameStatusEventArgs e)
        {
            GameStatusChanged?.Invoke(this, e);
        }

        public static readonly Dictionary<AC_STATUS, string> StatusNameLookup = new()
        {
            { AC_STATUS.AC_OFF, "Off" },
            { AC_STATUS.AC_LIVE, "Live" },
            { AC_STATUS.AC_PAUSE, "Pause" },
            { AC_STATUS.AC_REPLAY, "Replay" },
        };

        [SupportedOSPlatform("windows")]
        public AssettoCorsa()
        {
            sharedMemoryRetryTimer = new System.Timers.Timer(2000);
            sharedMemoryRetryTimer.AutoReset = true;
            sharedMemoryRetryTimer.Elapsed += sharedMemoryRetryTimer_Elapsed;

            physicsTimer = new System.Timers.Timer();
            physicsTimer.AutoReset = true;
            physicsTimer.Elapsed += physicsTimer_Elapsed;
            PhysicsInterval = 10;

            graphicsTimer = new System.Timers.Timer();
            graphicsTimer.AutoReset = true;
            graphicsTimer.Elapsed += graphicsTimer_Elapsed;
            GraphicsInterval = 1000;

            staticInfoTimer = new System.Timers.Timer();
            staticInfoTimer.AutoReset = true;
            staticInfoTimer.Elapsed += staticInfoTimer_Elapsed;
            StaticInfoInterval = 1000;

            Stop();
        }

        public void Start() => sharedMemoryRetryTimer.Start();

        [SupportedOSPlatform("windows")]
        private void sharedMemoryRetryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectToSharedMemory();
        }

        [SupportedOSPlatform("windows")] // ✅ добавлено — исправляет CA1416
        private bool ConnectToSharedMemory()
        {
            try
            {
                memoryStatus = AC_MEMORY_STATUS.CONNECTING;

                physicsMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_physics");
                graphicsMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_graphics");
                staticInfoMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_static");

                staticInfoTimer.Start();
                ProcessStaticInfo();

                graphicsTimer.Start();
                ProcessGraphics();

                physicsTimer.Start();
                ProcessPhysics();

                sharedMemoryRetryTimer.Stop();
                memoryStatus = AC_MEMORY_STATUS.CONNECTED;
                return true;
            }
            catch (FileNotFoundException)
            {
                staticInfoTimer.Stop();
                graphicsTimer.Stop();
                physicsTimer.Stop();
                return false;
            }
        }

        public void Stop()
        {
            memoryStatus = AC_MEMORY_STATUS.DISCONNECTED;
            sharedMemoryRetryTimer.Stop();

            physicsTimer.Stop();
            graphicsTimer.Stop();
            staticInfoTimer.Stop();
        }

        public double PhysicsInterval
        {
            get => physicsTimer.Interval;
            set => physicsTimer.Interval = value;
        }

        public double GraphicsInterval
        {
            get => graphicsTimer.Interval;
            set => graphicsTimer.Interval = value;
        }

        public double StaticInfoInterval
        {
            get => staticInfoTimer.Interval;
            set => staticInfoTimer.Interval = value;
        }

        private MemoryMappedFile? physicsMMF;
        private MemoryMappedFile? graphicsMMF;
        private MemoryMappedFile? staticInfoMMF;

        private System.Timers.Timer physicsTimer;
        private System.Timers.Timer graphicsTimer;
        private System.Timers.Timer staticInfoTimer;

        public event PhysicsUpdatedHandler? PhysicsUpdated;
        public event GraphicsUpdatedHandler? GraphicsUpdated;
        public event StaticInfoUpdatedHandler? StaticInfoUpdated;

        public virtual void OnPhysicsUpdated(PhysicsEventArgs e)
        {
            PhysicsUpdated?.Invoke(this, e);
        }

        public virtual void OnGraphicsUpdated(GraphicsEventArgs e)
        {
            GraphicsUpdated?.Invoke(this, e);
            if (gameStatus != e.Graphics.Status)
            {
                gameStatus = e.Graphics.Status;
                GameStatusChanged?.Invoke(this, new GameStatusEventArgs(gameStatus));
            }
        }

        public virtual void OnStaticInfoUpdated(StaticInfoEventArgs e)
        {
            StaticInfoUpdated?.Invoke(this, e);
        }

        private void physicsTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => ProcessPhysics();
        private void graphicsTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => ProcessGraphics();
        private void staticInfoTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => ProcessStaticInfo();

        private void ProcessPhysics()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED) return;
            try
            {
                var physics = ReadPhysics();
                OnPhysicsUpdated(new PhysicsEventArgs(physics));
            }
            catch (AssettoCorsaNotStartedException) { }
        }

        private void ProcessGraphics()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED) return;
            try
            {
                var graphics = ReadGraphics();
                OnGraphicsUpdated(new GraphicsEventArgs(graphics));
            }
            catch (AssettoCorsaNotStartedException) { }
        }

        private void ProcessStaticInfo()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED) return;
            try
            {
                var staticInfo = ReadStaticInfo();
                OnStaticInfoUpdated(new StaticInfoEventArgs(staticInfo));
            }
            catch (AssettoCorsaNotStartedException) { }
        }

        public Physics ReadPhysics()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED || physicsMMF == null)
                throw new AssettoCorsaNotStartedException();

            using var stream = physicsMMF.CreateViewStream();
            using var reader = new BinaryReader(stream);
            var size = Marshal.SizeOf(typeof(Physics));
            var bytes = reader.ReadBytes(size);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (Physics)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Physics))!;
            handle.Free();
            return data;
        }

        public Graphics ReadGraphics()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED || graphicsMMF == null)
                throw new AssettoCorsaNotStartedException();

            using var stream = graphicsMMF.CreateViewStream();
            using var reader = new BinaryReader(stream);
            var size = Marshal.SizeOf(typeof(Graphics));
            var bytes = reader.ReadBytes(size);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (Graphics)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Graphics))!;
            handle.Free();
            return data;
        }

        public StaticInfo ReadStaticInfo()
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED || staticInfoMMF == null)
                throw new AssettoCorsaNotStartedException();

            using var stream = staticInfoMMF.CreateViewStream();
            using var reader = new BinaryReader(stream);
            var size = Marshal.SizeOf(typeof(StaticInfo));
            var bytes = reader.ReadBytes(size);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (StaticInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(StaticInfo))!;
            handle.Free();
            return data;
        }
    }
}
