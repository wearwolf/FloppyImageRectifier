Imports System.IO

Public Class ScpTrackOffset
    Public Property TrackNumber As Byte
    Public Property Offset As UInteger

    Public Sub Read(binReader As BinaryReader, trackNumber As Byte)
        Me.TrackNumber = trackNumber
        Offset = binReader.ReadUInt32()
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        Dim side = TrackNumber Mod 2
        Dim actualTrackNumber = (TrackNumber \ 2) + 1
        outputWriter.WriteLine($"Track Number = {TrackNumber} (Side {side} - Track {actualTrackNumber})")
        outputWriter.WriteLine($"offset = {Offset}")
    End Sub
End Class
