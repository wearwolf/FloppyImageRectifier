Imports System.IO

Public Class ImgFile
    Public Property FilePath As String

    Public Property Sectors As List(Of ImgSector)

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        Sectors = New List(Of ImgSector)
    End Sub

    Public Sub Read()
        Using fstream = File.OpenRead(FilePath)
            Using binReader = New BinaryReader(fstream)
                Dim sectorNumber = 0
                While fstream.Position < fstream.Length
                    Dim sector = New ImgSector(sectorNumber)
                    sector.Read(binReader)
                    Sectors.Add(sector)

                    sectorNumber += 1
                End While
            End Using
        End Using
    End Sub

    Public Sub Write()
        Using fstream = File.Create(FilePath)
            Using binWriter = New BinaryWriter(fstream)
                For Each sector In Sectors
                    sector.Write(binWriter)
                Next
            End Using
        End Using
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        For Each sector In Sectors
            sector.WriteOutput(outputWriter)
        Next
    End Sub
End Class
