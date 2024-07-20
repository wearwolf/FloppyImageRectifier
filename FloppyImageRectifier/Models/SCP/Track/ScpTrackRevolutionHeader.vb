Imports System.IO

Public Class ScpTrackRevolutionHeader
    Public Property RevolutionNumber As Integer

    Public Property IndexTime As UInteger
    Public Property Length As UInteger
    Public Property Offset As UInteger

    Public Sub New(revolutionNumber As Integer)
        Me.RevolutionNumber = revolutionNumber
    End Sub

    Public Sub Read(binReader As BinaryReader)
        IndexTime = binReader.ReadUInt32()
        Length = binReader.ReadUInt32()
        Offset = binReader.ReadUInt32()
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Revolution Number = {RevolutionNumber}")
        outputWriter.WriteLine($"INDEX TIME = {IndexTime}")
        outputWriter.WriteLine($"TRACK LENGTH = {Length}")
        outputWriter.WriteLine($"DATA OFFSET = {Offset}")
    End Sub
End Class
