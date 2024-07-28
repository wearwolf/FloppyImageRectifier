Public Class MfmFluxDecoder

#Region "Constants"
    Private Const CLOCK_ADJUST_RATIO = 0.01

    Private Const PC_MFM_525_360_RPM = 300
    Private Const PC_MFM_525_360_BITCELL_LENGTH_RAD = 0.0001257
    Private Const PC_MFM_525_1200_RPM = 360
    Private Const PC_MFM_525_1200_BITCELL_LENGTH_RAD = 0.0000755
    Private Const PC_MFM_35_720_RPM = 300
    Private Const PC_MFM_35_720_BITCELL_LENGTH_RAD = 0.0001257
    Private Const PC_MFM_35_1440_RPM = 300
    Private Const PC_MFM_35_1440_BITCELL_LENGTH_RAD = 0.0000628

    Private Const SINGLE_ZERO_LOWER_BOUND = 0.8
    Private Const DOUBLE_ZERO_LOWER_BOUND = 1.3
    Private Const DOUBLE_ZERO_AVERAGE = 1.475
    Private Const TRIPLE_ZERO_LOWER_BOUND = 1.85
    Private Const TRIPLE_ZERO_AVERAGE = 2.05
    Private Const TRIPLE_ZERO_UPPER_BOUND = 2.25

#End Region

#Region "Fields"

    Private m_diskRpm As Integer
    Private m_diskBitcellLengthRad As Double
    Private m_nominalBitcellTime As Double
    Private m_tickResolution As Double

    Private m_maxBitcellTime As Double
    Private m_minBitcellTime As Double

    Private m_currentBitcellTime As Double

#End Region

#Region "Constructor"

    Public Sub New(diskType As FloppyDiskType, tickResolution As Double)
        Select Case (diskType)
            Case FloppyDiskType.PC_MFM_525_360
                m_diskRpm = PC_MFM_525_360_RPM
                m_diskBitcellLengthRad = PC_MFM_525_360_BITCELL_LENGTH_RAD
            Case FloppyDiskType.PC_MFM_525_1200
                m_diskRpm = PC_MFM_525_1200_RPM
                m_diskBitcellLengthRad = PC_MFM_525_1200_BITCELL_LENGTH_RAD
            Case FloppyDiskType.PC_MFM_35_720
                m_diskRpm = PC_MFM_35_720_RPM
                m_diskBitcellLengthRad = PC_MFM_35_720_BITCELL_LENGTH_RAD
            Case FloppyDiskType.PC_MFM_35_1440
                m_diskRpm = PC_MFM_35_1440_RPM
                m_diskBitcellLengthRad = PC_MFM_35_1440_BITCELL_LENGTH_RAD
        End Select

        m_tickResolution = tickResolution

        Dim diskSpeedRadPerSecond = (m_diskRpm * 2 * Math.PI) / 60
        m_nominalBitcellTime = m_diskBitcellLengthRad / diskSpeedRadPerSecond
        m_maxBitcellTime = m_nominalBitcellTime * 1.3
        m_minBitcellTime = m_nominalBitcellTime * 0.7

        m_currentBitcellTime = m_nominalBitcellTime
    End Sub

#End Region

#Region "Public Methods"

    Public Function Decode(timingList As List(Of Long)) As BitList
        Dim bitList = New BitList()

        For Each tick In timingList
            Dim time = tick * m_tickResolution
            Dim bitCells = time / m_currentBitcellTime
            If bitCells < SINGLE_ZERO_LOWER_BOUND Then
                Continue For
            ElseIf bitCells < DOUBLE_ZERO_LOWER_BOUND Then
                bitList.AddBit(False)
                AddAverageTime(time)
            ElseIf bitCells < TRIPLE_ZERO_LOWER_BOUND Then
                bitList.AddBit(False)
                bitList.AddBit(False)
                Dim newTime = time / DOUBLE_ZERO_AVERAGE
                AddAverageTime(newTime)
            ElseIf bitCells < TRIPLE_ZERO_UPPER_BOUND Then
                bitList.AddBit(False)
                bitList.AddBit(False)
                bitList.AddBit(False)
                Dim newTime = time / TRIPLE_ZERO_AVERAGE
                AddAverageTime(newTime)
            Else
                Dim zeros = CInt((bitCells / 0.5)) - 2
                For i = 0 To zeros
                    bitList.AddBit(False)
                Next
            End If

            bitList.AddBit(True)
        Next

        Return bitList
    End Function

#End Region

#Region "Private Methods"

    Private Sub AddAverageTime(time As Double)
        m_currentBitcellTime += (time - m_currentBitcellTime) * CLOCK_ADJUST_RATIO
        m_currentBitcellTime = Math.Max(m_minBitcellTime, Math.Min(m_currentBitcellTime, m_maxBitcellTime))
    End Sub

#End Region
End Class
