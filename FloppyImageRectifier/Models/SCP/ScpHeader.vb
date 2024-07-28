Imports System.IO

Public Class ScpHeader
    Private Const SIGNATURE_LENGTH = 3
    Private Const EXPECTED_SIGNATURE = "SCP"
    Private Const BASE_CAPTURE_RESOLUTION = 0.000000025

    Public Property Signature As String
    Public Property VersionRevision As Byte
    Public Property DiskType As ScpDiskType
    Public Property NumberOfRevolutionsPerTrack As Byte
    Public Property StartTrack As Byte
    Public Property EndTrack As Byte
    Public Property Flags As ScpImageFlags
    Public Property BitCellWidth As Byte
    Public Property HeadNumber As ScpHeads
    Public Property CaptureResolution As Byte
    Public Property Checksum As UInteger

    Public Property CaptureResolutionInSeconds As Double

    Public Sub Read(binReader As BinaryReader)
        Signature = binReader.ReadChars(SIGNATURE_LENGTH)

        If Signature <> EXPECTED_SIGNATURE Then
            Throw New InvalidOperationException($"SCP file Signature '{Signature}' different than expected signature of '{EXPECTED_SIGNATURE}' ")
        End If

        VersionRevision = binReader.ReadByte()
        DiskType = binReader.ReadByte()
        NumberOfRevolutionsPerTrack = binReader.ReadByte()
        StartTrack = binReader.ReadByte()
        EndTrack = binReader.ReadByte()
        Flags = binReader.ReadByte()
        BitCellWidth = binReader.ReadByte()
        HeadNumber = binReader.ReadByte()
        CaptureResolution = binReader.ReadByte()
        Checksum = binReader.ReadUInt32()

        CaptureResolutionInSeconds = (CaptureResolution + 1) * BASE_CAPTURE_RESOLUTION
    End Sub

    Public Sub Write(binWriter As BinaryWriter)
        binWriter.Write(Signature.ToCharArray())
        binWriter.Write(VersionRevision)
        binWriter.Write(DiskType)
        binWriter.Write(NumberOfRevolutionsPerTrack)
        binWriter.Write(StartTrack)
        binWriter.Write(EndTrack)
        binWriter.Write(Flags)
        binWriter.Write(BitCellWidth)
        binWriter.Write(HeadNumber)
        binWriter.Write(CaptureResolution)
        binWriter.Write(Checksum)
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Signature = {Signature}")
        outputWriter.WriteLine($"VERSION = {VersionRevision:X2}")
        outputWriter.WriteLine($"DISK TYPE = {DiskType}")
        outputWriter.WriteLine($"NUMBER OF REVOLUTIONS = {NumberOfRevolutionsPerTrack}")
        outputWriter.WriteLine($"START TRACK = {StartTrack}")
        outputWriter.WriteLine($"END TRACK = {EndTrack}")
        outputWriter.WriteLine($"FLAGS BITS = {Flags}")
        Dim actualBitCellWidth = If(BitCellWidth = 0, 16, BitCellWidth)
        outputWriter.WriteLine($"BIT CELL ENCODING = {BitCellWidth} ({actualBitCellWidth} bits)")
        outputWriter.WriteLine($"NUMBER OF HEADS = {HeadNumber}")
        outputWriter.WriteLine($"RESOLUTION = {CaptureResolution} ({(CaptureResolution + 1) * 25} ns)")
        outputWriter.WriteLine($"CHECKSUM = {Checksum:X8}")
    End Sub
End Class


