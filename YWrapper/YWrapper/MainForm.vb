Imports System.IO
Public Class MainForm
    Shared Function GetCommandLineArgs() As System.Collections.ObjectModel.ReadOnlyCollection(Of String)
        Dim value As String = Environment.CommandLine
        Dim bInsideQuote As Boolean
        Dim iCnt As Integer
        If value = "" Then Return New System.Collections.ObjectModel.ReadOnlyCollection(Of String)(Nothing)
        For Each c As Char In value.ToCharArray
            iCnt += 1
            If c = """"c Then
                bInsideQuote = Not bInsideQuote
                Continue For
            End If
            If c = " " Then
                If bInsideQuote Then
                    Continue For
                Else
                    Mid$(value, iCnt, 1) = "Â§"
                End If
            End If
        Next
        'Replace multiple separators
        While value.IndexOf("Â§Â§") > 0
            value = value.Replace("Â§Â§", "Â§")
        End While
        Dim CmdArgs As New List(Of String)
        Dim FirstArg As Boolean = True
        For Each sArg As String In value.Split("Â§".ToCharArray)
            If FirstArg Then
                FirstArg = False
                Continue For
            End If
            CmdArgs.Add(sArg)
        Next
        Return New System.Collections.ObjectModel.ReadOnlyCollection(Of String)(CmdArgs)
    End Function

    Shared Sub Main()

        'Get current exe informations
        Dim ThisExeFullPath As String = Application.ExecutablePath                          'Get the full path of current exe
        Dim FileInfo As New FileInfo(ThisExeFullPath)                                       'set as an object
        Dim FileName As String = FileInfo.Name                                              'get the name of the current exe

        'Declare variable and populate values
        'From Config

        Dim Exename As String = Environment.ExpandEnvironmentVariables(My.Settings.ExeName) 'read config file for exe name (Expand environment variable) (String)   
        Dim Constructor As String = My.Settings.Constructor                                 'read config file for constructor string (String)
        Dim Separator As Char = My.Settings.ParamChar                                       'read config file for Param Char (Char)
        Dim Wait As Boolean = My.Settings.Wait                                              'read config file for Wait (Boolean)
        Dim Hidden As Boolean = My.Settings.Hidden                                          'read config file for Hidden (Boolean)
        Dim Verbose As Boolean = My.Settings.Verbose                                        'read config file for verbose (Boolean)

        'From command line

        'Dim cmdLineParams As String() = Environment.GetCommandLineArgs()                    'get command line arguments sent to this (String())
        'Dim argcount As Int32 = cmdLineParams.Count - 1                                     'count command line argument (Integer)

        Dim cmdLineParams As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = GetCommandLineArgs()
        Dim argcount As Int32 = cmdLineParams.Count

        '############################################################################################
        '                   PROGRAM
        '############################################################################################

        '## SET windows style
        Dim Style As AppWinStyle = AppWinStyle.NormalFocus                                  'set windows style to "normal"
        If Hidden Then                                                                      'if Hidden as been set
            Style = AppWinStyle.Hide                                                        'set windows style to "hidden"
        End If

        '## SET Command Line Arguments
        'Prevent no arg error
        If cmdLineParams(0) = "" And argcount = 1 Then
            argcount = 0                                                                    'to prevent no arg = 1
        End If

        'Store constructor in FinalArgLine
        Dim FinalArgLine As String = Constructor                                            'put constructor in a new string

        For Each Argument As String In cmdLineParams                                        'Loop through received arguments
            Dim NextParam_StartPos As Int32 = FinalArgLine.IndexOf(Separator)               'Search for next separator char
            If NextParam_StartPos = -1 Then
                FinalArgLine = FinalArgLine & " " & Argument                                'No more separator char found put argument as is
            Else
                Dim NextParam_StopPos As Int32 = FinalArgLine.IndexOf(" ", NextParam_StartPos)  'get next space after found separator
                If NextParam_StopPos = -1 Then
                    NextParam_StopPos = FinalArgLine.Length                                 'Last separator char = no more space = use end of string as delimiter
                End If
                Dim StrToRepl As String = FinalArgLine.Substring(NextParam_StartPos, NextParam_StopPos - NextParam_StartPos)
                FinalArgLine = FinalArgLine.Replace(StrToRepl, Argument)
            End If
        Next

        Dim DoNext As Boolean = True
        Do Until Not DoNext
            Dim NextSeparator As Int32 = FinalArgLine.IndexOf(Separator)                    'Search for Orphean Separator in constructor
            If -Not NextSeparator = -1 Then
                Dim NextSpace As Int32 = FinalArgLine.IndexOf(" ", NextSeparator)
                If NextSpace = -1 Then
                    NextSpace = FinalArgLine.Length
                End If
                Dim StrToRepl As String = FinalArgLine.Substring(NextSeparator, NextSpace - NextSeparator)
                FinalArgLine = FinalArgLine.Replace(StrToRepl, "")
            Else
                DoNext = False
            End If
        Loop
        'Create the full command line
        Dim ToRun As String = Exename & " " & FinalArgLine                                   'construct command line (append exename to FinalArgLine)

        'Display verbose informations
        If Verbose Then
            MsgBox("Command Line received : " & vbCrLf _
                 & Environment.CommandLine _
                 & vbCrLf & vbCrLf & vbCrLf _
                 & "Command Line Constructor : " & vbCrLf _
                 & Constructor _
                 & vbCrLf & vbCrLf _
                 & "ParamChar : " & vbCrLf _
                 & Separator _
                 & vbCrLf & vbCrLf _
                 & "Command Line sent : " & vbCrLf _
                 & ToRun, MsgBoxStyle.OkOnly, FileName)
        End If
        Try
            Shell(ToRun, Style, Wait)
        Catch ex As Exception
            MsgBox(ex.Message & vbCrLf & vbCrLf _
                & "Command Line sent : " & vbCrLf _
                & ToRun & vbCrLf _
                , MsgBoxStyle.OkOnly, FileName)
        End Try

        FileInfo = Nothing

    End Sub
End Class
