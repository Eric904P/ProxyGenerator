Imports System.IO
Imports System.Windows.Forms
Imports ProxyGenerator.My.Resources

'Public Class Program
Module Program
    ReadOnly Scraper As New Scraper
    ReadOnly Checker As New Checker
    'Dim Progress As Progress

    'Save dialogue
    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = v2_Threading_SaveFile_txt_files____txt____txt
            fs.FilterIndex = 1
            fs.ShowDialog()
            If Not (fs.FileName = Nothing) Then
                Using sw As New StreamWriter(fs.FileName)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            Console.WriteLine(v2_Threading_SaveFile_Please_wait_until_program_is_finished_)
        End If
    End Sub

    Sub Main()
        'Progress = New Progress()
        Scraper.Load()
        If Scraper.ScrapeHerder() Then
            If Checker.CheckHerder() Then
                SaveFile(Checker.ReturnWorking())
            End If
        End If
    End Sub
End Module
'End Class


