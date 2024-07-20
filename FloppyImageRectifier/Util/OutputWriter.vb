Imports System.IO

Public Class OutputWriter
    Implements IDisposable

    Private m_file As StreamWriter
    Private disposedValue As Boolean

    Public Sub New(outputPath As String)
        If Not String.IsNullOrEmpty(outputPath) Then
            m_file = New StreamWriter(outputPath)
        End If
    End Sub

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
End Class
