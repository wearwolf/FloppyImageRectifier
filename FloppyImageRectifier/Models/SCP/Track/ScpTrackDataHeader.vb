Imports System.IO

Public Class ScpTrackDataHeader
    Private Const SIGNATURE_LENGTH = 3
    Private Const EXPECTED_SIGNATURE = "TRK"

    Public Property Signature As String
    Public Property TrackNumber As Byte

    Public Sub Read(binReader As BinaryReader)
        Signature = binReader.ReadChars(SIGNATURE_LENGTH)

        If Signature <> EXPECTED_SIGNATURE Then
            Throw New InvalidOperationException($"SCP track Signature '{Signature}' different than expected signature of '{EXPECTED_SIGNATURE}' ")
        End If

        TrackNumber = binReader.ReadByte()
    End Sub

    Friend Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Signature = {Signature}")
        Dim side = TrackNumber Mod 2
        Dim actualTrackNumber = (TrackNumber \ 2) + 1
        outputWriter.WriteLine($"TrackNumber = {TrackNumber}  (Side {side} - Track {actualTrackNumber})")
    End Sub
End Class
