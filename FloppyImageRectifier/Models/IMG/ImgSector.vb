Imports System.IO

Public Class ImgSector
    Private Const SECTOR_SIZE = 512

    Public Property SectorNumber As Integer
    Public Property Data As List(Of Byte)

    Public Sub New(sectorNumber As Integer)
        Me.SectorNumber = sectorNumber
    End Sub

    Public Sub Read(binReader As BinaryReader)
        Data = New List(Of Byte)(binReader.ReadBytes(SECTOR_SIZE))
    End Sub

    Public Sub Write(binWriter As BinaryWriter)
        binWriter.Write(Data.ToArray())
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Sector Number = {SectorNumber}")
        outputWriter.PrintBytes(Data, 16, 0, OutputWriter.BytePrinterFormat.Hex)
    End Sub
End Class
