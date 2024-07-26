Imports System.IO
Imports System.Text

Public Class OutputWriter
    Implements IDisposable

#Region "Enums"

    Enum BytePrinterFormat
        Hex
        BinaryBigEndian
        BinaryLittleEndian
    End Enum

#End Region

#Region "Fields"

    Private m_file As StreamWriter
    Private disposedValue As Boolean

#End Region

#Region "Constructor"

    Public Sub New(outputPath As String)
        If Not String.IsNullOrEmpty(outputPath) Then
            m_file = New StreamWriter(outputPath)
        End If
    End Sub

#End Region

#Region "Public Methods"

    Public Sub Write(text As String)
        Console.Write(text)
        If m_file IsNot Nothing Then
            m_file.Write(text)
        End If
    End Sub

    Public Sub WriteLine()
        Console.WriteLine()
        If m_file IsNot Nothing Then
            m_file.WriteLine()
        End If
    End Sub

    Public Sub WriteLine(text As String)
        Console.WriteLine(text)
        If m_file IsNot Nothing Then
            m_file.WriteLine(text)
        End If
    End Sub

    Sub PrintBytes(data As List(Of Byte), bytesPerRow As Integer, bytesPerGroup As Integer, format As BytePrinterFormat)
        Dim sb = New StringBuilder()

        Dim blockCount = 1
        Dim index = 0
        Dim sb2 = New StringBuilder()

        For Each dataByte In data
            If bytesPerGroup <> 0 AndAlso index Mod bytesPerGroup = 0 Then
                sb.AppendLine()
                sb.AppendLine($"---- {blockCount} ----")
                blockCount += 1
            End If

            Dim byteString As String = String.Empty

            Select Case format
                Case BytePrinterFormat.Hex
                    byteString = dataByte.ToString("X2") + " "

                    If Char.IsControl(Chr(dataByte)) Then
                        sb2.Append(" ")
                    Else
                        sb2.Append(Chr(dataByte))
                    End If

                Case BytePrinterFormat.BinaryBigEndian, BytePrinterFormat.BinaryLittleEndian
                    byteString = ToBinaryString(dataByte, format)
            End Select

            sb.Append(byteString)

            index += 1
            If (bytesPerGroup <> 0 AndAlso index Mod bytesPerGroup = 0) _
                                OrElse (bytesPerRow <> 0 AndAlso index Mod bytesPerRow = 0) Then
                If format = BytePrinterFormat.Hex Then
                    sb.Append("    " + sb2.ToString())
                    sb2.Clear()
                End If

                sb.AppendLine()
            End If
        Next
        sb.AppendLine()
        Write(sb.ToString())
    End Sub

#End Region

#Region "IDisposable"

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                m_file.Dispose()
            End If

            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region "Helper Methods"

    Private Function ToBinaryString(dataByte As Byte, format As BytePrinterFormat) As String
        Dim sb = New StringBuilder

        For i = 0 To 7
            Dim bit As Boolean
            If format = BytePrinterFormat.BinaryBigEndian Then
                bit = (dataByte >> (7 - i)) Mod 2 = 1
            Else
                bit = (dataByte >> i) Mod 2 = 1
            End If

            If bit Then
                sb.Append("1")
            Else
                sb.Append("0")
            End If
        Next

        Return sb.ToString()
    End Function

#End Region

End Class
