using NMCMotionSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmce
{
    public class MmceManager
    {
        private readonly Dictionary<ushort, MmceMaster> _masterMap = new Dictionary<ushort, MmceMaster>();

        public void UpdateMasterMap()
        {
            _masterMap.Clear();

            var count = ushort.MinValue;
            NMCSDKLib.MC_GetMasterCount(ref count).ThrowExceptionIfNotOk();

            var dataArray = new ushort[count];
            NMCSDKLib.MC_GetMasterMap(dataArray, ref count);

            foreach (var id in dataArray)
            {
                var master = new MmceMaster(id);

                _masterMap[id] = master;
            }
        }
    }

    public enum MmceMasterState : uint
    {
        /// <summary>
        /// 정지 상태 : SCAN, EtherCAT Network Configuration, Firmware Download 가 가능한 상태
        /// </summary>
        Idle = 0,
        /// <summary>
        /// Slave Device 를 SCAN 하고 있는 상태 - EtherCAT Network Configuration, Firmware Download 가 불가능한 상태
        /// </summary>
        Scan,
        /// <summary>
        /// EtherCAT network 이 RUN 상태로 동작중인 상태
        /// </summary>
        Run,
        /// <summary>
        ///  상태 변경 중
        /// </summary>
        InTransition,
        /// <summary>
        /// Error 상태
        /// </summary>
        Error,
        /// <summary>
        /// Master 와 Slave Deivce 간 Network Link 가 깨진 상태
        /// </summary>
        Linkbroken,
        /// <summary>
        /// ???
        /// </summary>
        FreeRun, //v12.1.0.54
    }

    public class MmceMaster
    {
        private readonly Dictionary<ushort, MmceSlave> _slaveMap = new Dictionary<ushort, MmceSlave>();
        private readonly Dictionary<ushort, MmceAxis> _axisMap = new Dictionary<ushort, MmceAxis>();
        private readonly ushort _boardid;


        public MmceMaster(ushort boardid)
        {
            _boardid = boardid;
        }


        public MmceMasterState GetState()
        {
            var data = byte.MinValue;
            NMCSDKLib.MasterGetCurMode(_boardid, ref data).ThrowExceptionIfNotOk();

            return (MmceMasterState)data;
        }

        public void UpdateSlaveMap()
        {
            _slaveMap.Clear();

            var count = ushort.MinValue;
            NMCSDKLib.MasterGetDeviceCount(_boardid, ref count).ThrowExceptionIfNotOk();

            var dataArray = new ushort[count];
            NMCSDKLib.MasterGetDeviceID(_boardid, dataArray).ThrowExceptionIfNotOk();

            foreach (var id in dataArray)
            {
                var slave = new MmceSlave(_boardid, id);

                _slaveMap[id] = slave;
            }
        }

        public void UpdateAxisMap()
        {
            _axisMap.Clear();

            var count = ushort.MinValue;
            NMCSDKLib.MasterGetAxesCount(_boardid, ref count).ThrowExceptionIfNotOk();

            var dataArray = new ushort[count];
            NMCSDKLib.MasterGetAxesID(_boardid, dataArray).ThrowExceptionIfNotOk();

            foreach (var id in dataArray)
            {
                var axis = new MmceAxis(_boardid, id);

                _axisMap[id] = axis;
            }
        }
    }

    public enum MmceSlaveGetIOTypes : byte
    {
        Out,
        In
    }

    public enum MmceSlaveState : uint
    {
        /// <summary>
        /// 인식 불가
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 초기화
        /// </summary>
        Init = 0x01,    //Ver_0c010016_2
        /// <summary>
        /// 동작전
        /// </summary>
        PreOp = 0x02,   //Ver_0c010016_2
        /// <summary>
        /// ???
        /// </summary>
        BootStrap = 0x03,   //v12.1.0.48
        /// <summary>
        /// 안전동작
        /// </summary>
        SafeOp = 0x04,  //Ver_0c010016_2
        /// <summary>
        /// 정상동작
        /// </summary>
        Op = 0x08,  //ver_0c010016_2
        /// <summary>
        /// 에러 
        /// </summary>
        AckErr = 0x10   //Ver_0c010016_2
    }

    public class MmceSlave
    {
        private readonly ushort _boardid;
        private readonly ushort _slaveid;

        public MmceSlave(ushort boardid, ushort slaveid)
        {
            _boardid = boardid;
            _slaveid = slaveid;
        }


        public MmceSlaveState GetState()
        {
            var data = byte.MinValue;
            NMCSDKLib.SlaveGetCurState(_boardid, _slaveid, ref data).ThrowExceptionIfNotOk();

            return (MmceSlaveState)data;
        }

        public void SetSdoData(ushort sdoIndex, byte subIndex, byte[] dataArray)
        {
            var resp = uint.MinValue;
            NMCSDKLib.MasterSetSDODataEcatAddr(_boardid, _slaveid, sdoIndex, subIndex, (uint)dataArray.Length, ref resp, dataArray).ThrowExceptionIfNotOk();
        }

        public byte[] GetSdoData(ushort sdoIndex, byte subIndex, uint dataSize)
        {
            var resp = uint.MinValue;
            var dataArray = new byte[dataSize];
            NMCSDKLib.MasterGetSDODataEcatAddr(_boardid, _slaveid, sdoIndex, subIndex, (uint)dataArray.Length, ref resp, dataArray).ThrowExceptionIfNotOk();

            return dataArray;
        }

        private byte GetIOByte(MmceSlaveGetIOTypes ioType, uint offset)
        {
            var data = byte.MinValue;
            NMCSDKLib.MC_IO_READ_BYTE(_boardid, _slaveid, (ushort)ioType, offset, ref data).ThrowExceptionIfNotOk();

            return data;
        }

        private ushort GetIOWord(MmceSlaveGetIOTypes ioType, uint offset)
        {
            var data = ushort.MinValue;
            NMCSDKLib.MC_IO_READ_WORD(_boardid, _slaveid, (ushort)ioType, offset, ref data).ThrowExceptionIfNotOk();

            return data;
        }

        private uint GetIODoubleWord(MmceSlaveGetIOTypes ioType, uint offset)
        {
            var data = uint.MinValue;
            NMCSDKLib.MC_IO_READ_DWORD(_boardid, _slaveid, (ushort)ioType, offset, ref data).ThrowExceptionIfNotOk();

            return data;
        }

        private void SetIOByte(uint offset, byte data)
        {
            NMCSDKLib.MC_IO_WRITE_BYTE(_boardid, _slaveid, offset, data).ThrowExceptionIfNotOk();
        }

        private void SetIOWord(uint offset, ushort data)
        {
            NMCSDKLib.MC_IO_WRITE_WORD(_boardid, _slaveid, offset, data).ThrowExceptionIfNotOk();
        }

        private void SetIODoubleWord(uint offset, uint data)
        {
            NMCSDKLib.MC_IO_WRITE_DWORD(_boardid, _slaveid, offset, data).ThrowExceptionIfNotOk();
        }
    }

    [Flags]
    public enum MmceAxisState : uint
    {
        /// <summary>
        /// 축의 Error상태로 모션 동작이 불가능한 상태
        /// </summary>
        ErrorStop = 0x00000001,
        /// <summary>
        /// 축의 Amp Off 상태로 모션 동작이 불가능한 상태
        /// </summary>
        Disabled = 0x00000002,
        /// <summary>
        /// MC_Stop에 의해 감속정지 중 또는 감속정지는 완료 되었지만 MC_Stop의 Execute가 유지되어 있는 상태로 모션 동작이 불가능한 상태
        /// </summary>
        Stopping = 0x00000004,
        /// <summary>
        /// 축의 Amp On 상태로 모션 동작이 가능한 준비 상태
        /// </summary>
        StandStill = 0x00000008,
        /// <summary>
        /// MC_MoveAbsoulte, MC_MoveRelative, MC_Halt 등에 의해 단축 모션 중인 상태
        /// </summary>
        DiscreteMotion = 0x00000010,
        /// <summary>
        /// MC_MoveVelocity에 의해 지속적인 모션 중인 상태
        /// </summary>
        ContinuousMotion = 0x00000020,
        /// <summary>
        /// MC_GearIn 또는 GroupMotion에 의한 모션 상태
        /// </summary>
        SynchroMotion = 0x00000040,
        /// <summary>
        /// 축이 MC_Home에 의해 원점복귀 중인 상태
        /// </summary>
        Homing = 0x00000080,
        /// <summary>
        /// Software Position Positive Limit Value를 벗어나면 On으로 변경
        /// </summary>
        SWLimitSwitchPosEvent = 0x00000100,
        /// <summary>
        /// Software Position Negative Limit Value를 벗어나면 On으로 변경
        /// </summary>
        SWLimitSwitchNegEvent = 0x00000200,
        /// <summary>
        /// 모션이 등속동작 중인 상태
        /// </summary>
        ConstantVelocity = 0x00000400,
        /// <summary>
        /// 모션이 가속동작 중인 상태
        /// </summary>
        Accelerating = 0x00000800,
        /// <summary>
        /// 모션이 감속동작 중인 상태
        /// </summary>
        Decelerating = 0x00001000,
        /// <summary>
        /// 모션이 정방향으로 동작 중인 상태(모션이 정지 중일 때 Default로 동작)
        /// </summary>
        DirectionPositive = 0x00002000,
        /// <summary>
        /// 모션이 역방향으로 동작 중인 상태
        /// </summary>
        DirectionNegative = 0x00004000,
        /// <summary>
        /// Negative Hardware Limit Switch의 상태
        /// </summary>
        LimitSwitchNeg = 0x00008000,
        /// <summary>
        /// Positive Hardware Limit Switch의 상태
        /// </summary>
        LimitSwitchPos = 0x00010000,
        /// <summary>
        /// Home Switch의 상태
        /// </summary>
        HomeAbsSwitch = 0x00020000,
        /// <summary>
        /// Positive Hardware Limit Switch가 Active 되었을 때 On으로 변경
        /// </summary>
        LimitSwitchPosEvent = 0x00040000,
        /// <summary>
        /// Negative Hardware Limit Switch가 Active 되었을 때 On으로 변경
        /// </summary>
        LimitSwitchNegEvent = 0x00080000,
        /// <summary>
        /// 해당 축에 링크된 Slave Drive가 Fault상태(Slave Statusword bit 3)
        /// </summary>
        DriveFault = 0x00100000,
        /// <summary>
        /// 해당 축에 설정된 SensorStop에 의해 모션이 정지된 상태
        /// </summary>
        SensorStop = 0x00200000,
        /// <summary>
        /// AmpOn이 가능한 상태
        /// </summary>
        ReadyForPowerOn = 0x00400000,
        /// <summary>
        /// AmpOn이 가능한 상태
        /// </summary>
        PowerOn = 0x00800000,
        /// <summary>
        /// MC_Home에 의해 원점복귀가 완료된 상태
        /// </summary>
        IsHomed = 0x01000000,
        /// <summary>
        /// 모션 동작에는 문제가 없지만 축의 Warning상태
        /// </summary>
        AxisWarning = 0x02000000,
        /// <summary>
        /// 모션 동작이 완료된 Inposition 상태
        /// </summary>
        MotionComplete = 0x04000000,
        /// <summary>
        /// MC_GearIn에 의해 Gearing 동작 중인 상태
        /// </summary>
        Gearing = 0x08000000,
        /// <summary>
        /// 해당 축이 그룹에 속한 상태이며 해당 그룹이 Enable인 상태
        /// </summary>
        GroupMotion = 0x10000000,
        /// <summary>
        /// 해당 축의 Buffer 1000개가 모두 사용된 상태
        /// </summary>
        BufferFull = 0x20000000,
        /// <summary>
        /// Reserved for Future Use
        /// </summary>
        Reserved_as_30 = 0x40000000,
        /// <summary>
        /// 해당 축에 링크된 Slave Drive의 Sensor data의 버그 상태
        /// </summary>
        DebugLimitEvent = 0x80000000,
    }

    [Flags]
    public enum MmceAxisSimpleState : uint
    {
        /// <summary>
        /// 축의 Error상태로 모션 동작이 불가능한 상태
        /// </summary>
        ErrorStop = 0x00000001,
        /// <summary>
        /// 축의 Amp Off 상태로 모션 동작이 불가능한 상태
        /// </summary>
        Disabled = 0x00000002,
        /// <summary>
        /// MC_Stop에 의해 감속정지 중 또는 감속정지는 완료 되었지만 MC_Stop의 Execute가 유지되어 있는 상태로 모션 동작이 불가능한 상태
        /// </summary>
        Stopping = 0x00000004,
        /// <summary>
        /// 축의 Amp On 상태로 모션 동작이 가능한 준비 상태
        /// </summary>
        StandStill = 0x00000008,
        /// <summary>
        /// MC_MoveAbsoulte, MC_MoveRelative, MC_Halt 등에 의해 단축 모션 중인 상태
        /// </summary>
        DiscreteMotion = 0x00000010,
        /// <summary>
        /// MC_MoveVelocity에 의해 지속적인 모션 중인 상태
        /// </summary>
        ContinuousMotion = 0x00000020,
        /// <summary>
        /// MC_GearIn 또는 GroupMotion에 의한 모션 상태
        /// </summary>
        SynchroMotion = 0x00000040,
        /// <summary>
        /// 축이 MC_Home에 의해 원점복귀 중인 상태
        /// </summary>
        Homing = 0x00000080,
    }

    public struct MmceAxisError
    {
        public ushort ID { get; set; }
        public ushort Info { get; set; }
        public ushort SubInfo { get; set; }
    }

    public enum MmceAxisActualParameterTypes : uint
    {
        CommandedPosition = 1,
        CommandedVelocity = 11,
        CommandedAccel = 1001,
        CommandedJerk = 1003,
    }

    public enum MmceAxisCommandedParameterTypes : uint
    {
        CommandedPosition = 1,
        CommandedVelocity = 11,
        CommandedAccel = 1001,
        CommandedJerk = 1003,
    }

    public class MmceAxis
    {
        private readonly ushort _boardid;
        private readonly ushort _axisid;

        public MmceAxis(ushort boardid, ushort axisid)
        {
            _boardid = boardid;
            _axisid = axisid;
        }


        public MmceAxisState GetState()
        {
            var data = uint.MinValue;

            NMCSDKLib.MC_ReadAxisStatus(_boardid, _axisid, ref data).ThrowExceptionIfNotOk();

            return (MmceAxisState)data;
        }

        public MmceAxisSimpleState GetSimpleState()
        {
            var data = uint.MinValue;

            NMCSDKLib.MC_ReadStatus(_boardid, _axisid, ref data).ThrowExceptionIfNotOk();

            return (MmceAxisSimpleState)data;
        }

        public void SetPowerOn()
        {
            NMCSDKLib.MC_Power(_boardid, _axisid, true).ThrowExceptionIfNotOk();
        }

        public void SetPowerOff()
        {
            NMCSDKLib.MC_Power(_boardid, _axisid, false).ThrowExceptionIfNotOk();
        }

        public void SetErrorClear()
        {
            NMCSDKLib.MC_Reset(_boardid, _axisid).ThrowExceptionIfNotOk();
        }

        public void SetPosition(double position, bool relative)
        {
            NMCSDKLib.MC_Reset(_boardid, _axisid).ThrowExceptionIfNotOk();
            NMCSDKLib.MC_SetPosition(_boardid, _axisid, position, relative, NMCSDKLib.MC_EXECUTION_MODE.mcImmediately);
        }

        public double GetActualPosition()
        {
            var data = double.MinValue;
            NMCSDKLib.MC_ReadActualPosition(_boardid, _axisid, ref data).ThrowExceptionIfNotOk();

            return data;
        }

        public double GetActualVelocity()
        {
            var data = double.MinValue;
            NMCSDKLib.MC_ReadActualVelocity(_boardid, _axisid, ref data).ThrowExceptionIfNotOk();

            return data;
        }

        public double GetActualParameter(MmceAxisActualParameterTypes type)
        {
            var data = double.MinValue;

            NMCSDKLib.MC_ReadParameter(_boardid, _axisid, (uint)type, ref data);

            return data;
        }

        public double GetCommandedParameter(MmceAxisCommandedParameterTypes type)
        {
            var data = double.MinValue;

            NMCSDKLib.MC_ReadParameter(_boardid, _axisid, (uint)type, ref data);

            return data;
        }

        public MmceAxisError GetErrorInfo()
        {
            var id = ushort.MinValue;
            var info = ushort.MinValue;
            var subinfo = ushort.MinValue;

            NMCSDKLib.MC_ReadAxisError(_boardid, _axisid, ref id, ref info, ref subinfo);

            return new MmceAxisError { ID = id, Info = info, SubInfo = subinfo };
        }
    }

    internal static class MmceExtensions
    {
        public static void ThrowExceptionIfNotOk(this NMCSDKLib.MC_STATUS instance)
        {

        }
    }
}
