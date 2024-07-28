Imports System.IO

Public Class HfeHeader
    Public Const EXPECTED_SIGNATURE = "HXCPICFE"

    Private Const SIGNATURE_LENGTH = 8
    Private Const HEADER_LENGTH = 26

    Public Property Signature As String
    Public Property FormatVersion As Byte
    Public Property NumberOfTracks As Byte
    Public Property NumberOfSides As Byte
    Public Property TrackEncoding As HfeTrackEncoding
    Public Property BitRate As UShort
    Public Property FloppyRpm As UShort
    Public Property FloppyInterfaceMode As HfeFloppyInterfaceMode
    Public Property Dnu As Byte
    Public Property TrackListOffset As UShort
    Public Property WriteAllowed As Boolean
    Public Property SingleStep As HfeStep
    Public Property Track0s0AltEncoding As Boolean
    Public Property Track0s0Encoding As HfeTrackEncoding
    Public Property Track0s1AltEncoding As Boolean
    Public Property Track0s1Encoding As HfeTrackEncoding

    Public Sub Read(binReader As BinaryReader)
        Signature = binReader.ReadChars(SIGNATURE_LENGTH)

        If Signature <> EXPECTED_SIGNATURE Then
            Throw New InvalidOperationException($"SCP file Signature '{Signature}' different than expected signature of '{EXPECTED_SIGNATURE}' ")
        End If

        FormatVersion = binReader.ReadByte()
        NumberOfTracks = binReader.ReadByte()
        NumberOfSides = binReader.ReadByte()
        TrackEncoding = binReader.ReadByte()
        BitRate = BitConverter.ToUInt16(binReader.ReadBytes(2))
        FloppyRpm = BitConverter.ToUInt16(binReader.ReadBytes(2))
        FloppyInterfaceMode = binReader.ReadByte()
        Dnu = binReader.ReadByte()
        TrackListOffset = BitConverter.ToUInt16(binReader.ReadBytes(2))
        WriteAllowed = binReader.ReadByte() = 0
        SingleStep = binReader.ReadByte()
        Track0s0AltEncoding = binReader.ReadByte() = 0
        Track0s0Encoding = binReader.ReadByte()
        Track0s1AltEncoding = binReader.ReadByte() = 0
        Track0s1Encoding = binReader.ReadByte()
    End Sub

    Public Sub Write(binWriter As BinaryWriter)
        binWriter.Write(Signature.ToCharArray())
        binWriter.Write(FormatVersion)
        binWriter.Write(NumberOfTracks)
        binWriter.Write(NumberOfSides)
        binWriter.Write(TrackEncoding)
        binWriter.Write(BitRate)
        binWriter.Write(FloppyRpm)
        binWriter.Write(FloppyInterfaceMode)
        binWriter.Write(Dnu)
        binWriter.Write(TrackListOffset)
        binWriter.Write(If(WriteAllowed, CByte(0), CByte(&HFF)))
        binWriter.Write(SingleStep)
        binWriter.Write(If(Track0s0AltEncoding, CByte(0), CByte(&HFF)))
        binWriter.Write(Track0s0Encoding)
        binWriter.Write(If(Track0s1AltEncoding, CByte(0), CByte(&HFF)))
        binWriter.Write(Track0s1Encoding)

        For i = 1 To HfeFile.BLOCK_LENGTH - HEADER_LENGTH
            binWriter.Write(CByte(&HFF))
        Next
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"HEADERSIGNATURE = {Signature}")
        outputWriter.WriteLine($"formatrevision = {FormatVersion}")
        outputWriter.WriteLine($"number_of_track = {NumberOfTracks}")
        outputWriter.WriteLine($"number_of_side = {NumberOfSides}")
        outputWriter.WriteLine($"track_encoding = {TrackEncoding}")
        outputWriter.WriteLine($"bitRate = {BitRate}")
        outputWriter.WriteLine($"floppyRPM = {FloppyRpm}")
        outputWriter.WriteLine($"floppyinterfacemode = {FloppyInterfaceMode}")
        outputWriter.WriteLine($"dnu = {Dnu}")
        outputWriter.WriteLine($"track_list_offset = {TrackListOffset}")

        If WriteAllowed Then
            outputWriter.WriteLine($"write_allowed = Yes")
        Else
            outputWriter.WriteLine($"write_allowed = No")
        End If

        outputWriter.WriteLine($"single_step = {SingleStep}")

        If Track0s0AltEncoding Then
            outputWriter.WriteLine($"track0s0_altencoding = Yes")
        Else
            outputWriter.WriteLine($"track0s0_altencoding = No")
        End If
        outputWriter.WriteLine($"track0s0_encoding = {Track0s0Encoding}")

        If Track0s1AltEncoding Then
            outputWriter.WriteLine($"track0s1_altencoding = Yes")
        Else
            outputWriter.WriteLine($"track0s1_altencoding = No")
        End If
        outputWriter.WriteLine($"track0s1_encoding = {Track0s1Encoding}")
    End Sub
End Class
