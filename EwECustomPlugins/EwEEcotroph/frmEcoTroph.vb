' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

Option Strict Off
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Xml
Imports System.Xml.Serialization
Imports EcoTroph.cEcotrophPlugin
Imports EwECore
Imports EwECore.WebServices
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports Ionic.Zip
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

'not relevent to uncomppress R_ET.zip folder
'Imports Shell32

' ================================================================================
' Ecotroph code audit 1, 21Jun2013, Jeroen Steenbeek
'
' Recommended changes:
' - All lengthy operations should provide status feedback via cApplicationStatusNotifier
' - All try/catch blocks should write an entry to cLog
' - Use the EwEUtils R interop class for interacting with R
' - Use a class to transfer and analyze R results (now in result_tab) that
'   1) Gathers result strings in a wrapped buffer protected from index out of range issues
'   2) Provides command and communication status
' - Globalize all user legible texts
' ================================================================================

Public Class frmEcotroph

    Private num_model() As Integer
    Private aide As String = "http://ecobase.ecopath.org/index.php?action=examples&lang=uk"
    Private m_strRPath As String = ""
    Private m_strRRoot As String = ""

    Public Sub New()
        Me.InitializeComponent()
        Me.m_strRRoot = cSystemUtils.ApplicationSettingsPath
        'JG 14/11/2013 For all the profiles of all the students i can't install R for Ecopath (disk space and storing in the Roaming profile 
        'is not good
        'So if the R directory as been installed by the administrator i use this install instead of download and store in each roaming profiles

        If (My.Computer.FileSystem.FileExists(Path.Combine(CurDir(), "R\bin\i386\r.exe"))) Then
            Me.m_strRPath = Path.Combine(CurDir(), "R\bin\i386\r.exe")
        Else
            Me.m_strRPath = Path.Combine(Me.m_strRRoot, "R\bin\i386\r.exe")
        End If

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Debug.Assert(Me.UIContext IsNot Nothing)

        Dim test() As String = Nothing
        Dim result() As String = Nothing
        Dim result_tab() As String
        ' Dim repos As String = "http://mirror.ibcp.fr/pub/CRAN/bin/windows/contrib/2.14"
        Dim repos_simple As String = "http://cran.univ-lyon1.fr/"
        Dim repos As String = repos_simple & "bin/windows/contrib/2.14/"
        Dim bSucces As Boolean = True

        'We have to test first if R is present in the Ewe directory
        ReDim test(6)
        ' We need to check 1- the version of R 2,3,4- If a new version of the Package exist and if we need to upgrade it
        test(0) = "getRversion()"
        test(1) = "is.element('EcoTroph',installed.packages()[,1])"
        test(2) = "options(timeout=1);summary(packageStatus(repositories=c('" & repos & "')))$inst$Version['EcoTroph']"
        test(3) = "Etat<-summary(packageStatus(repositories=c('" & repos & "')))$inst"
        test(4) = "Etat[Etat$Package=='EcoTroph','Status']"
        test(5) = "installed.packages()['EcoTroph','Version']"

        bSucces = execute_r(test, result)
        result_tab = Split(result(1), vbCr)

        If (bSucces = False) Then

            ' ToDo: globalize this, please...
            If (Me.AskUser("You don't have R installed, you won't be able to run Ecotroph! Do you wish to download and install the minimum R for ecotroph directory now?", eMessageReplyStyle.YES_NO) = eMessageReply.OK) Then

                Dim ETdownload As String = cFileUtils.MakeTempFile(".zip")

                ' ToDo: globalize this!
                cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, "Downloading local copy of R...", -1)
                Try
                    ' ToDo: Download et.zip exe into Temp folder, and install. This requires administrator rights
                    '       - To unzip we do NOT need unzip.exe. We can use Ionic.zip.dll, provided in ScIntShared. This removes one security vulnerability.
                    '       - To download et.zip we do not need administrator rights.
                    '       - To write et files we do need administrator rights. I have added a test in this form
                    My.Computer.Network.DownloadFile("http://sirs.agrocampus-ouest.fr/EcoTroph/data/R_ET.zip", ETdownload, "", "", True, 500, True)
                Catch ex As Exception
                    Me.NotifyUser(cStringUtils.Localize(My.Resources.PB_DOWNLOAD, ex.Message), eMessageImportance.Critical)
                    cLog.Write(ex, "frmEcotroph.OnLoad(download_r)")
                End Try
                cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)

                ' ToDo: globalize this!
                cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, "Installing local copy of R...", -1)
                Try
                    Dim zip As New Ionic.Zip.ZipFile(ETdownload)

                    ' ToDo: globalize this!
                    If (Me.AskUser("Do you want to install the R for Ecotroph minimal application in the EwE directory (need administrator right) ? if no it will be install in your profile.", eMessageReplyStyle.YES_NO) = eMessageReply.OK) Then
                        zip.ExtractAll(CurDir())
                        Me.m_strRPath = Path.Combine(CurDir(), "R\bin\i386\r.exe")
                    Else
                        zip.ExtractAll(Me.m_strRRoot)
                        Me.m_strRPath = Path.Combine(Me.m_strRRoot, "R\bin\i386\r.exe")
                    End If

                Catch ex As Exception
                    cLog.Write(ex, "frmEcoTroph.OnLoad(install_r)")
                End Try
                cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)

                ' Test R version again to see if package needs updating
                bSucces = execute_r(test, result)
                result_tab = Split(result(1), vbCr)

            End If
        Else
            ' Scary
            ecotroph_version.Text = result_tab(6)
        End If

        If (result_tab.Length > 3) Then
            If (result_tab(4).Contains("upgrade")) Then

                ' ToDo: globalize this!
                If (Me.AskUser("A new version of the EcoTroph R package is available. Do you wish to upgrade now?", eMessageReplyStyle.YES_NO) = eMessageReply.OK) Then

                    cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, "Upgrading R package...", -1)
                    Try
                        test(0) = " install.packages('EcoTroph',repos=c('" & repos_simple & "'))"
                        test(1) = ""
                        test(2) = ""
                        test(3) = ""
                        test(4) = ""
                        execute_r(test, result)
                    Catch ex As Exception
                        cLog.Write(ex, "frmEcotroph.OnLoad(upgrade_r)")
                    End Try
                    cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)
                End If
            End If
        End If

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
        smooth_pdf = Nothing
        result_pdf = Nothing
        result_pdf_et_diag = Nothing
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Load_from_ecopath.Click

        ' Try to load a model
        If Not Me.Core.StateMonitor.HasEcopathLoaded Then

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("LoadEcopathModel")
            If cmd IsNot Nothing Then
                cmd.Invoke()
            End If
        End If

        If Not Me.Core.StateMonitor.HasEcopathLoaded Then
            Return
        End If

        'a retester ou alors tester si les données sont dispo
        EcoTroph.cEcotrophPlugin.etCore.RunEcoPath()

        ETgridinput.BringToFront()

        If Not (IsNothing(ETinputdatafromEP.TL)) Then

            Dim DataGrid As DataGridView = Me.ETgridinput
            'Ajout de cela pour corriger bug decouvert 06/11/2014 quand on chargeait 2 fois le modèle, il mettait input data à null
            Me.ETgridinput.Rows.Clear()

            For igrp As Integer = 0 To ETinputdatafromEP.TL.Length - 2
                If (DataGrid.RowCount < ETinputdatafromEP.TL.Length) Then
                    DataGrid.Rows.Add()
                End If
                DataGrid.Item(0, igrp).Value() = ETinputdatafromEP.GroupName(igrp + 1)
                DataGrid.Item(1, igrp).Value() = ETinputdatafromEP.TL(igrp + 1)
                DataGrid.Item(2, igrp).Value() = ETinputdatafromEP.B(igrp + 1)
                DataGrid.Item(3, igrp).Value() = ETinputdatafromEP.PROD(igrp + 1)
                DataGrid.Item(4, igrp).Value() = ETinputdatafromEP.accessibility(igrp + 1)
                DataGrid.Item(5, igrp).Value() = ETinputdatafromEP.OI(igrp + 1)
            Next
            commentaires.Text = ETinputdata.NumFleet

            DataGrid.ColumnCount = 6 + ETinputdatafromEP.NumFleet
            For ifleet As Integer = 0 To ETinputdatafromEP.NumFleet - 1
                DataGrid.Columns(6 + ifleet).Name = ETinputdatafromEP.FleetName(ifleet + 1)
                For igrp As Integer = 0 To ETinputdatafromEP.TL.Length - 2
                    DataGrid.Item(6 + ifleet, igrp).Value() = ETinputdatafromEP.Catches(ifleet)(igrp + 1)

                Next
                DataGrid.Columns(4).DefaultCellStyle.BackColor = Drawing.Color.BurlyWood
            Next
            'ETinputdata = ETinputdatafromEP
            ETinputdata.NumFleet = ETinputdatafromEP.NumFleet
            'If Not (IsNothing(ETinputdata.comments)) Then commentaires.Text = ETinputdata.comments Else commentaires.Text = ""
            If Not (IsNothing(ETinputdata.ModelName)) Then Modelname.Text = ETinputdata.ModelName Else Modelname.Text = ""
            If Not (IsNothing(ETinputdata.ModelDescription)) Then modeldescription.Text = ETinputdata.ModelDescription Else modeldescription.Text = ""
            Button2.Enabled = True
            Button3.Enabled = True
            Button4.Enabled = True
        Else
            Me.NotifyUser(My.Resources.NO_MODEL_DATA, eMessageImportance.Critical)
        End If

        ' frmET.ETgridinput.DataSource = ETinput
        ' frmET.ETgridinput.Show()
    End Sub


    Private Sub Save_ETdata_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Save_ETdata.Click
        Dim saveFileDialog1 As New SaveFileDialog()

        saveFileDialog1.Filter = My.Resources.FILEFILTER_XML
        saveFileDialog1.Title = My.Resources.SAVE_ECOTROPH
        saveFileDialog1.ShowDialog()
        ETinputdata.Comments = commentaires.Text
        ETinputdata.ModelName = Modelname.Text
        ETinputdata.ModelDescription = modeldescription.Text

        ' If the file name is not an empty string open it for saving.
        If saveFileDialog1.FileName <> "" Then
            ' Saves the Image via a FileStream created by the OpenFile method.
            'Due to a bug with special charachter (# or < in description try to add encoding
            'JG 07/10/2013
            'Old version
            'Dim writer As New System.Xml.Serialization.XmlSerializer(GetType(ETinputtot))
            'Dim file As New System.IO.StreamWriter(saveFileDialog1.FileName)

            'writer.Serialize(file, ETinputdata)
            'file.Close()
            Try
                Dim serializer As New XmlSerializer(GetType(ETinputtot))
                Dim fs As New FileStream(saveFileDialog1.FileName, FileMode.Create)
                Dim writer As New XmlTextWriter(fs, Encoding.Unicode)
                serializer.Serialize(writer, ETinputdata)
                writer.Close()
            Catch ex As Exception

            End Try
        End If

    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim myStream As Stream = Nothing
        Dim openFileDialog1 As New OpenFileDialog()
        Dim reader As New System.Xml.Serialization.XmlSerializer(GetType(ETinputtot))
        openFileDialog1.InitialDirectory = "c:\"
        openFileDialog1.Filter = My.Resources.FILEFILTER_XML
        openFileDialog1.FilterIndex = 2
        openFileDialog1.RestoreDirectory = True
        ETgridinput.BringToFront()

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try

                Dim file As New System.IO.StreamReader(openFileDialog1.FileName)
                If (openFileDialog1.FileName <> "") Then
                    ETinputdata = CType(reader.Deserialize(file), ETinputtot)
                End If
            Catch Ex As Exception
                Me.NotifyUser(cStringUtils.Localize(My.Resources.ERROR_INPUT_FILE, Ex.Message), eMessageImportance.Critical)
            Finally
                ' Check this again, since we need to make sure we didn't throw an exception on open.
                If (myStream IsNot Nothing) Then
                    myStream.Close()
                End If
            End Try
        End If

        If (openFileDialog1.FileName <> "") Then


            Dim DataGrid As DataGridView = Me.ETgridinput
            'List faut une procédure pour afficher cela
            For igrp As Integer = 0 To ETinputdata.TL.Length - 2
                If (DataGrid.RowCount < ETinputdata.TL.Length) Then
                    DataGrid.Rows.Add()
                End If

                DataGrid.Item(0, igrp).Value() = ETinputdata.GroupName(igrp + 1)
                DataGrid.Item(1, igrp).Value() = ETinputdata.TL(igrp + 1)
                DataGrid.Item(2, igrp).Value() = ETinputdata.B(igrp + 1)
                DataGrid.Item(3, igrp).Value() = ETinputdata.PROD(igrp + 1)

                If Not (IsNothing(ETinputdata.accessibility)) Then DataGrid.Item(4, igrp).Value() = ETinputdata.accessibility(igrp + 1)
                If Not (IsNothing(ETinputdata.OI)) Then DataGrid.Item(5, igrp).Value() = ETinputdata.OI(igrp + 1)

            Next
            If Not (IsNothing(ETinputdata.Comments)) Then commentaires.Text = ETinputdata.Comments Else commentaires.Text = ""
            If Not (IsNothing(ETinputdata.ModelName)) Then Modelname.Text = ETinputdata.ModelName Else Modelname.Text = ""
            If Not (IsNothing(ETinputdata.ModelDescription)) Then modeldescription.Text = ETinputdata.ModelDescription Else modeldescription.Text = ""
            DataGrid.ColumnCount = 6 + ETinputdata.NumFleet
            For ifleet As Integer = 0 To ETinputdata.NumFleet - 1
                DataGrid.Columns(6 + ifleet).Name = ETinputdata.FleetName(ifleet)
                For igrp As Integer = 0 To ETinputdata.TL.Length - 2
                    DataGrid.Item(6 + ifleet, igrp).Value() = ETinputdata.Catches(ifleet)(igrp + 1)
                Next

            Next
            DataGrid.Columns(4).DefaultCellStyle.BackColor = Drawing.Color.BurlyWood
        End If
        Button2.Enabled = True
        Button3.Enabled = True
        Button4.Enabled = True
    End Sub

    Private Sub ETgridinput_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles ETgridinput.CellValueChanged
        'MsgBox("on est sur " & e.ColumnIndex & e.ColumnIndex)
        If (e.ColumnIndex >= 0 And e.RowIndex >= 0) Then
            '  MsgBox(Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).ToString)
            Try

                Select Case e.ColumnIndex
                    Case 0
                        ETinputdata.GroupName(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                    Case 1
                        ETinputdata.TL(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                    Case 2
                        ETinputdata.B(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                    Case 3
                        ETinputdata.PROD(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                    Case 4
                        ETinputdata.accessibility(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                    Case 5
                        ETinputdata.OI(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                        'Then it's fleet catches
                    Case Is > 5

                        ETinputdata.Catches(e.ColumnIndex - 6)(e.RowIndex + 1) = Me.ETgridinput.Item(e.ColumnIndex, e.RowIndex).Value
                End Select
            Catch ex As Exception
                ' ToDo: globalize this
                ' ToDo: use EwE messaging system please
                Me.NotifyUser(cStringUtils.Localize("Problem in modifying data, do you respect decimal seperator? {0}", ex.Message), eMessageImportance.Warning)
                cLog.Write(ex, "frmEcotroph.ETgridinput_CellValueChanged")
            End Try

        End If

    End Sub

    Private Sub RadioButton1_CheckedChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles type_smooth1.CheckedChanged
        Me.GroupBox2.Visible = False
        Me.parameters_cst.Visible = True

    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles type_smooth2.CheckedChanged
        Me.GroupBox2.Visible = True
        Me.parameters_cst.Visible = False
    End Sub

    Private Sub RadioButton3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles type_smooth3.CheckedChanged
        Me.GroupBox2.Visible = False
        Me.parameters_cst.Visible = False
    End Sub

    Public Function execute_r(ByVal code As String(), ByRef result As String()) As Boolean

        ' ToDo: use EwE messaging system instead of MsgBox, please

        'Cette fonction execute un code R et renvoie le nom d'un fichier resultat
        Dim myProcess As New Process()
        Dim bSucces As Boolean = True

        myProcess.StartInfo.UseShellExecute = False ' A remettre à false

        myProcess.StartInfo.RedirectStandardInput = True
        myProcess.StartInfo.RedirectStandardOutput = True
        myProcess.StartInfo.RedirectStandardError = True

        myProcess.StartInfo.FileName = Me.m_strRPath
        myProcess.StartInfo.Arguments = "--slave"
        myProcess.StartInfo.CreateNoWindow = True

        Dim output2() As String
        ReDim output2(2)
        If IO.File.Exists(myProcess.StartInfo.FileName) Then
            Try
                myProcess.Start()
                Dim myStreamWriter As StreamWriter = myProcess.StandardInput
                For icod As Integer = 0 To code.Count - 1
                    myStreamWriter.WriteLine(code(icod))
                    Debug.Print(code(icod))
                Next
                myStreamWriter.Close()

                Dim depasse As Boolean = myProcess.WaitForExit(100000)
                If depasse Then
                    output2(1) = myProcess.StandardOutput.ReadToEnd()
                    output2(0) = myProcess.StandardError.ReadToEnd()
                Else
                    Me.NotifyUser(My.Resources.EXCEED_TIME_R, eMessageImportance.Warning)
                    bSucces = False
                End If

            Catch ex As Exception
                Me.NotifyUser(My.Resources.PB_R, eMessageImportance.Critical)
                cLog.Write(ex, "frmEcotroph.execute_r")
                bSucces = False
            End Try
        Else
            output2(0) = My.Resources.NO_R
            bSucces = False
        End If

        result = output2
        Return bSucces


    End Function

    Public Function execute_rplot(ByVal code As String()) As String
        'Cette fonction execute un code R et renvoie le nom d'un fichier resultat
        Dim myProcess As New Process()
        myProcess.StartInfo.RedirectStandardInput = False


        myProcess.StartInfo.UseShellExecute = True ' A remettre à false
        myProcess.StartInfo.FileName = Me.m_strRPath
        myProcess.StartInfo.Arguments = "--slave"
        myProcess.StartInfo.CreateNoWindow = False

        Try
            myProcess.Start()

            'Shell(myProcess.StartInfo.FileName)
            'Dim myStreamWriter As StreamWriter = myProcess.
            For icod As Integer = 0 To code.Count - 1
                My.Computer.Keyboard.SendKeys(code(icod))
                'myStreamWriter.WriteLine(code(icod))
                'MsgBox(myProcess.Threads.Count & "pour " & code(icod))
            Next
            'Dim output2 As String = myProcess.StandardError.ReadToEnd()
            'myStreamWriter.Close()
        Catch ex As Exception
            cLog.Write(ex, "frmEcotroph.execute_rplot")
            Return vbCancel
        End Try

        Return vbOK

    End Function

    Public Shared Function sauve_datagrid_xml(ByVal grille As ETinputtot, ByVal filename As String) As Boolean

        Dim serializer As New XmlSerializer(GetType(ETinputtot))
        Dim fs As New FileStream(filename, FileMode.Create)
        Dim writer As New XmlTextWriter(fs, Encoding.Unicode)
        serializer.Serialize(writer, ETinputdata)
        writer.Close()

        'Due to a bug with special charachter (# or < in description try to add encoding
        'JG 07/10/2013
        'Old version
        'Dim writer As New System.Xml.Serialization.XmlSerializer(GetType(ETinputtot))
        'Dim file_data As New System.IO.StreamWriter(filename)
        'writer.Serialize(file_data, ETinputdata)
        'file_data.Close()
        Return True

    End Function


    Public Shared Function charge_grid(ByVal donnees As String(), ByRef grille As DataGridView) As Integer

        Dim tab_trans(,) As String
        Dim uneligne() As String

        donnees(0) = vbTab & donnees(0)
        Dim nbl As Integer = donnees.Length
        Dim nbcol As Integer = (donnees(0).Split(vbTab).Length)


        ReDim tab_trans(nbcol, nbl)
        ReDim uneligne(nbcol)
        Dim deci_sep As String

        'Une astuce pour obtenir le sep décimal
        deci_sep = Mid$(CStr(1 / 2), 2, 1)


        ' La partie suivante est a mettre en fonction(donnees as data,sheet as )
        grille.ColumnCount = nbcol


        For igrp As Integer = 0 To nbl - 1

            If (grille.Rows.Count < nbl) Then grille.Rows.Add()
            uneligne = donnees(igrp).Split(vbTab)
            If uneligne.Length > 1 Then


                If (uneligne(1).Contains("mE")) Then
                    donnees(igrp) = donnees(igrp).Substring(1, donnees(igrp).Length - 1)
                    uneligne = donnees(igrp).Split(vbTab)
                End If
            End If
            For ielt As Integer = 0 To (uneligne.Count - 1)

                uneligne(ielt) = Replace(uneligne(ielt), ".", deci_sep)
                tab_trans(ielt, igrp) = uneligne(ielt)
                ' Ajout de l'arrondi mle 09/05/2012 sur 4 chiffre
                If (IsNumeric(uneligne(ielt))) Then
                    grille.Item(ielt, igrp).Value = Math.Round(CDbl(uneligne(ielt)), 4)
                Else
                    grille.Item(ielt, igrp).Value = uneligne(ielt)
                End If
                'grille.Item(ielt, igrp).Value = uneligne(ielt) si je veux vraiment voir les vrai chiffre 
            Next
        Next
        grille.Columns(0).DefaultCellStyle.BackColor = Drawing.Color.Gray
        grille.Rows(0).DefaultCellStyle.BackColor = Drawing.Color.Gray
        grille.RowCount = nbl
        Return (vbOK)
    End Function


    Public Shared Function get_params(ByVal type_smooth As Integer, Optional ByVal smooth_parameter As Double = Nothing, Optional ByVal decalage As Double = Nothing) As String
        'Cette fonction doit récupérer les paramètre du smooth
        Dim output2 As String = ""

        Select Case type_smooth
            Case 1
                output2 = ",sigmaLN_cst=" & Replace(smooth_parameter, ",", ".")
            Case 2
                output2 = ",smooth_type=2,smooth_param=" & Replace(smooth_parameter, ",", ".") & ",shift=" & Replace(decalage, ",", ".")
            Case 3
                output2 = ",smooth_type=3"
        End Select
        Return (output2)

    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        'On commence par sauver le fichier de données 

        Dim commandes() As String
        Dim fichierpdf As String = cFileUtils.MakeTempFile(".pdf")
        Dim fichier_data_transfert As String = cFileUtils.MakeTempFile(".xml")
        Dim fichier As String = cFileUtils.MakeTempFile(".txt")

        sauve_datagrid_xml(ETinputdata, fichier_data_transfert)

        'on charge les différents paramètres du create.smooth
        Dim param_pas As String = ""

        If (type_smooth1.Checked) Then param_pas = get_params(1, smooth_param_1.Text)
        If (type_smooth2.Checked) Then param_pas = get_params(2, smooth_param.Text, decalage.Text)
        If (type_smooth3.Checked) Then param_pas = get_params(3)


        'MsgBox("Nous allons Lancer la fonction smooth avec les paramètres :" & param_pas)
        'MsgBox("Nous allons Lancer la fonction smooth avec les paramètres :" & param_pas)

        'Le code R en lui même
        ReDim commandes(9)
        commandes(0) = ""
        commandes(1) = "library(EcoTroph)"

        commandes(2) = "read.ecopath.model_2015 <-function(filename){if (missing(filename))  { cat('filename is missing\n')} else {" & _
            "top <- xmlRoot(xmlTreeParse(filename,useInternalNodes=TRUE));" & _
            "xmlName(top) ;" & _
            "names(top) ;" & _
            "groupname<-as.vector(xmlSApply(top[['GroupName']],xmlValue));" & _
            "v<-xmlSApply(top,function(x) as.vector(xmlSApply(x,xmlValue)));" & _
            "catches_tmp<-xmlSApply(top[['Catches']],function(x) as.numeric(xmlSApply(x,xmlValue)));" & _
            "catches_tmp2<-data.frame(catches_tmp[1:v$NumFleet])[1:length(groupname),];" & _
            "names(catches_tmp2)<-paste('catch',v$FleetName[-length(v$FleetName)]);" & _
            "ecopath<-data.frame(v$GroupName,as.numeric(v$TL),as.numeric(v$B),as.numeric(v$PROD),as.numeric(v$accessibility),as.numeric(v$OI));" & _
            "names(ecopath)<-c('GroupName','TL','B','PROD','accessibility','OI');" & _
            "if (is.null(dim(catches_tmp2))) {ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath))]));" & _
            "names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI',paste('catch.',v$FleetName[-length(v$FleetName)],sep=''))};" & _
            "if (!is.null(dim(catches_tmp2))) {names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI');ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath)),]))};" & _
            "return (ecopath[!(ecopath$group_name==''),])}};"


        commandes(3) = "ecopath<-read.ecopath.model_2015('" & Replace(fichier_data_transfert, "\", "\\") & "')"
        commandes(4) = "A<-create.smooth(ecopath" & param_pas & ")"

        commandes(5) = "write.table(A, file ='" & Replace(fichier, "\", "\\") & "', sep = '\t',quote=FALSE)"
        commandes(6) = "pdf(file='" & Replace(fichierpdf, "\", "\\") & "')"

        'commandes(7) = "plot_smooth(A)" modification et utilisation plot générique
        commandes(7) = "plot(A)"
        commandes(8) = "dev.off()"
        commandes(9) = "quit('yes')"

        'on execute ce code R

        Try
            Dim output2() As String = Nothing
            execute_r(commandes, output2)
            ' If Len(output2) > 0 Then MsgBox(output2)

        Catch ex As Exception
            cLog.Write(ex, "frmEcoTroph.Button2_Click(execute_r_1)")
        End Try


        smooth_pdf.Navigate(fichierpdf)


        'smooth_pdf.Refresh()
        If My.Computer.FileSystem.FileExists(fichier) Then
            Dim recup() As String = File.ReadAllLines(fichier)
            Try

                charge_grid(recup, datasmooth)
            Catch ex As Exception
                Me.NotifyUser(cStringUtils.Localize("Problem in reading R script output. {0}", ex.Message), eMessageImportance.Critical)
                cLog.Write(ex, "frmEcoTroph.Button2_Click(read_pdf)")
            End Try
        Else
            Me.NotifyUser(My.Resources.NO_OUTPUT_R, eMessageImportance.Critical)
        End If


        Cursor.Current = Cursors.Default

        'Test de la partie graphique, pour voir

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim commandes() As String



        Dim fichierpdf As String = cFileUtils.MakeTempFile(".pdf")
        Dim fichier_data_transfert As String = cFileUtils.MakeTempFile(".xml")
        Dim fichier As String = cFileUtils.MakeTempFile(".txt")
        Dim log_ech As String



        result_pdf.GoHome()

        Cursor.Current = Cursors.WaitCursor


        'Juste pour attendre que le composant web ne bloque pas le fichier qui doit être mis à jour
        Dim param_pas As String = ""
        If (type_smooth1.Checked) Then param_pas = get_params(1, smooth_param_1.Text)

        If (type_smooth2.Checked) Then param_pas = get_params(2, smooth_param.Text, decalage.Text)
        If (type_smooth3.Checked) Then param_pas = get_params(3)
        ' MsgBox("Nous allons Lancer la fonction smooth avec les paramètres :" & param_pas)
        If (Log_scale.Checked) Then log_ech = ",scale1=log,scale2=log,scale3=log" Else log_ech = ""

        sauve_datagrid_xml(ETinputdata, fichier_data_transfert)

        'on charge les différents paramètres du create.smooth


        'Le code R en lui même



        fichier = Replace(fichier, "\", "\\")

        ReDim commandes(21)
        commandes(0) = "options(warn=0)"
        commandes(1) = "library(EcoTroph)"
        commandes(2) = "read.ecopath.model_2015 <-function(filename){if (missing(filename))  { cat('filename is missing\n')} else {" & _
          "top <- xmlRoot(xmlTreeParse(filename,useInternalNodes=TRUE));" & _
          "xmlName(top) ;" & _
          "names(top) ;" & _
          "groupname<-as.vector(xmlSApply(top[['GroupName']],xmlValue));" & _
          "v<-xmlSApply(top,function(x) as.vector(xmlSApply(x,xmlValue)));" & _
          "catches_tmp<-xmlSApply(top[['Catches']],function(x) as.numeric(xmlSApply(x,xmlValue)));" & _
          "catches_tmp2<-data.frame(catches_tmp[1:v$NumFleet])[1:length(groupname),];" & _
          "names(catches_tmp2)<-paste('catch',v$FleetName[-length(v$FleetName)]);" & _
          "ecopath<-data.frame(v$GroupName,as.numeric(v$TL),as.numeric(v$B),as.numeric(v$PROD),as.numeric(v$accessibility),as.numeric(v$OI));" & _
          "names(ecopath)<-c('GroupName','TL','B','PROD','accessibility','OI');" & _
          "if (is.null(dim(catches_tmp2))) {ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath))]));" & _
          "names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI',paste('catch.',v$FleetName[-length(v$FleetName)],sep=''))};" & _
          "if (!is.null(dim(catches_tmp2))) {names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI');ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath)),]))};" & _
          "return (ecopath[!(ecopath$group_name==''),])}};"


        commandes(3) = "ecopath<-read.ecopath.model_2015('" & Replace(fichier_data_transfert, "\", "\\") & "');A<-create.ETmain(ecopath" & param_pas & ")"

        commandes(4) = "write.table(A$ET_Main[as.numeric(rownames(A$ET_Main))<6,], file ='" & fichier & "', sep = '\t',quote=FALSE)"
        commandes(5) = "cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(6) = "write.table(A$biomass[as.numeric(rownames(A$biomass))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE)"
        commandes(7) = "cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(8) = "write.table(A$biomass_acc[as.numeric(rownames(A$biomass_acc))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(9) = "write.table(A$prod[as.numeric(rownames(A$prod))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(10) = "write.table(A$prod_acc[as.numeric(rownames(A$prod_acc))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(11) = "write.table(as.data.frame(lapply(A$Y,rowSums)),file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(12) = "AY<-Reduce('+',A$Y);write.table(AY[as.numeric(rownames(AY))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)"
        commandes(13) = "for (pecheries in names(A$Y)) {write.table(A$Y[[pecheries]][as.numeric(rownames(A$Y[[pecheries]]))<6,], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)}"

        commandes(14) = ""
        commandes(15) = "pdf(file='" & Replace(fichierpdf, "\", "\\") & "')"
        'commandes(16) = "plot_ETmain(A" & log_ech & ")"
        commandes(16) = "plot(A" & log_ech & ")"
        commandes(17) = "dev.off()"

        commandes(18) = " "
        commandes(19) = " "
        commandes(20) = " "
        commandes(21) = " quit('yes')"


        'on execute ce code R

        Try
            Dim output2() As String = Nothing
            execute_r(commandes, output2)
            ' If Len(output2) > 0 Then MsgBox(output2)

        Catch ex As Exception
            'MessageBox.Show("Problem in R script: " & ex.Message)
            cLog.Write(ex, "frmEcoTroph.Button3_Click")
        End Try


        result_pdf.Navigate(fichierpdf)

        If My.Computer.FileSystem.FileExists(fichier) Then



            'End If

            Dim recup() As String = File.ReadAllLines(fichier)

            Dim totales As String = Join(recup, vbNewLine)
            Dim matrices() As String = Split(totales, "-----")



            Dim Ctr() As Control = Me.Controls.Find("Catch." & (ETinputdata.FleetName(0)), True)
            Try

                charge_grid(matrices(0).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_ET_main)
                charge_grid(matrices(1).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_biomass)
                charge_grid(matrices(2).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_biomass_acc)
                charge_grid(matrices(3).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_flow_p)
                charge_grid(matrices(4).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_flow_p_acc)
                charge_grid(matrices(5).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_y)
                '    If panel_result.TabPages.Count = 6 Then
                For compteur_fleet As Integer = 0 To ETinputdata.NumFleet - 1

                    Dim ctrl() As Control = panel_result.Controls.Find("Catch." & (ETinputdata.FleetName(compteur_fleet)), True)

                    If ctrl.Length = 0 Then

                        Dim myTabPage As New TabPage()
                        myTabPage.Text = "Catch." & (ETinputdata.FleetName(compteur_fleet))
                        myTabPage.Name = "tabCatch." & (ETinputdata.FleetName(compteur_fleet))
                        panel_result.TabPages.Add(myTabPage)
                        Dim dtg As New DataGridView
                        dtg.Name = "Catch." & (ETinputdata.FleetName(compteur_fleet))
                        dtg.Height = 391
                        dtg.Width = 782
                        dtg.Top = 6
                        dtg.Left = 3
                        dtg.Dock = DockStyle.Fill
                        panel_result.TabPages(compteur_fleet + 6).Controls.Add(dtg)
                        charge_grid(matrices(compteur_fleet + 6).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), dtg)
                    Else
                        charge_grid(matrices(compteur_fleet + 6).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ctrl(0))
                    End If


                Next

            Catch ex As Exception
                Me.NotifyUser(cStringUtils.Localize("Problem in reading R script output. {0}", ex.Message), eMessageImportance.Critical)
                cLog.Write(ex, "frmEcoTroph.Button3_Click(read_results)")
            End Try


        Else
            Me.NotifyUser(My.Resources.NO_OUTPUT_R, eMessageImportance.Critical)
        End If

        Cursor.Current = Cursors.Default

    End Sub


    Private Sub getgraphs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles getgraphs.CheckedChanged
        If getgraphs.Checked = True Then
            result_pdf.BringToFront()

            result_pdf.Visible = True
        Else : result_pdf.Visible = False
        End If

    End Sub

    Private Sub Button4_Click_3(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Button4_Click_4(ByVal sender As System.Object, ByVal e As System.EventArgs)


    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If getgraph_diag.Checked = True Then
            result_pdf_et_diag.Visible = True
        Else
            result_pdf_et_diag.Visible = False
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles smooth_graph.CheckedChanged
        If smooth_graph.Checked = True Then
            smooth_pdf.BringToFront()
            smooth_pdf.Visible = True
        Else
            smooth_pdf.Visible = False
        End If
    End Sub

    Private Sub Reset_smooth_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Reset_smooth.Click
        smooth_param_1.Text = "0.12"
        decalage.Text = "0.95"
        smooth_param.Text = "0.07"
    End Sub

    Private Sub reset_param_diag_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        TopD.Text = "0.2"
        formd.Text = "0.5"
        beta.Text = "0.1"
    End Sub

    Private Sub b_input_check_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles b_input_check.CheckedChanged
        If b_input_check.Checked Then
            beta.Enabled = True
            Forag.Checked = False
        Else
            beta.Enabled = False
        End If
    End Sub

    Private Sub Ponto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles diagnosis_page.Click

    End Sub

    Private Sub getgraph_diag_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles getgraph_diag.CheckedChanged
        If getgraph_diag.Checked = True Then
            result_pdf_et_diag.BringToFront()

            result_pdf_et_diag.Visible = True
        Else : result_pdf_et_diag.Visible = False
        End If
    End Sub

    Private Sub Button4_Click_2(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim commandes() As String
        Dim fichierpdf As String = cFileUtils.MakeTempFile(".pdf")
        Dim fichier_data_transfert As String = cFileUtils.MakeTempFile(".xml")
        Dim fichier As String = cFileUtils.MakeTempFile(".txt")
        Dim log_ech_diag As String

        Cursor.Current = Cursors.WaitCursor


        'result_pdf_et_diag.GoHome()

        sauve_datagrid_xml(ETinputdata, fichier_data_transfert)

        Dim param_iso As String = ""

        'on charge les différents paramètres du create.smooth
        Dim param_pas As String = ""
        If (type_smooth1.Checked) Then param_pas = get_params(1, smooth_param_1.Text)

        If (type_smooth2.Checked) Then param_pas = get_params(2, smooth_param.Text, decalage.Text)
        If (type_smooth3.Checked) Then param_pas = get_params(3)
        If (log_scale_diagnose.Checked) Then log_ech_diag = ",scale=log" Else log_ech_diag = ""
        Dim param_pas2 As String = ", TopD = " & Replace(TopD.Text, ",", ".") & ", FormD = " & Replace(formd.Text, ",", ".")

        If (b_input_check.Checked) Then param_pas2 = param_pas2 & ",B.Input=TRUE, Beta = " & Replace(beta.Text, ",", ".")
        'Au 20 Juillet 2013, la foraiging arena est retirée du package, pas mure .....+ tard
        'If (Forag.Checked) Then
        'param_pas2 = param_pas2 & ",Forag.A=TRUE, Kfeed = " & Replace(Kfeed.Text, ",", ".") & ", Ponto = " & Replace(Ponto.Text, ",", ".")
        'Else
        'param_pas2 = param_pas2 & ",Forag.A=FALSE"
        'End If
        Dim param_EMSY As String = param_pas2

        If (same_mf.Checked) Then param_pas2 = param_pas2 & ",same.mE=TRUE"
        If (Not All_group.Checked) Then

            Dim liste_group As String = ""

            For Each item As Object In list_group_diag.SelectedItems

                liste_group = liste_group & "'" & item.ToString & "',"
            Next
            If liste_group.Length > 0 Then
                param_pas2 = param_pas2 & ",Group=c(" & liste_group.Substring(0, liste_group.Length - 1) & ")"
            End If

        End If

        If (Not same_mf.Checked) Then
            Dim liste_group As String = ""

            If List_fleet1.SelectedItems.Count = 0 Then

                param_pas2 = param_pas2 & ",same.mE=TRUE"
                Me.NotifyUser(My.Resources.NO_SELECTED_FLEET, eMessageImportance.Warning)
                same_mf.Checked = True
            Else

                For Each item As Object In List_fleet1.SelectedItems

                    liste_group = liste_group & "'catch." & item.ToString().Replace(" ", ".") & "',"
                Next
                param_iso = param_iso & ",fleet.of.interest=c(" & liste_group.Substring(0, liste_group.Length - 1) & ")"
                liste_group = ""

            End If




        End If

        'MsgBox("Nous allons Lancer la fonction smooth avec les paramètres :" & param_pas & " et " & param_pas2)


        'Le code R en lui même


        fichier = Replace(fichier, "\", "\\")

        ReDim commandes(30)
        Dim liste_tables() As String = {"ET_Main_diagnose", "B", "B_acc", "P", "P_acc", "Kin", "Kin_acc", "Fish_mort", "Fish_mort_acc", "Y"}

        commandes(0) = "library(EcoTroph)"
        commandes(1) = "read.ecopath.model_2015 <-function(filename){if (missing(filename))  { cat('filename is missing\n')} else {" & _
         "top <- xmlRoot(xmlTreeParse(filename,useInternalNodes=TRUE));" & _
         "xmlName(top) ;" & _
         "names(top) ;" & _
         "groupname<-as.vector(xmlSApply(top[['GroupName']],xmlValue));" & _
         "v<-xmlSApply(top,function(x) as.vector(xmlSApply(x,xmlValue)));" & _
         "catches_tmp<-xmlSApply(top[['Catches']],function(x) as.numeric(xmlSApply(x,xmlValue)));" & _
         "catches_tmp2<-data.frame(catches_tmp[1:v$NumFleet])[1:length(groupname),];" & _
         "names(catches_tmp2)<-paste('catch',v$FleetName[-length(v$FleetName)]);" & _
         "ecopath<-data.frame(v$GroupName,as.numeric(v$TL),as.numeric(v$B),as.numeric(v$PROD),as.numeric(v$accessibility),as.numeric(v$OI));" & _
         "names(ecopath)<-c('GroupName','TL','B','PROD','accessibility','OI');" & _
         "if (is.null(dim(catches_tmp2))) {ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath))]));" & _
         "names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI',paste('catch.',v$FleetName[-length(v$FleetName)],sep=''))};" & _
         "if (!is.null(dim(catches_tmp2))) {names(ecopath)<-c('group_name','TL','biomass','prod','accessibility','OI');ecopath<-data.frame(ecopath,as.data.frame(catches_tmp2[1:length(rownames(ecopath)),]))};" & _
         "return (ecopath[!(ecopath$group_name==''),])}};"


        commandes(2) = "ecopath<-read.ecopath.model_2015('" & Replace(fichier_data_transfert, "\", "\\") & "');ETM<-create.ETmain(ecopath" & param_pas & ");A<-create.ETdiagnosis(ETM" & param_pas2 & param_iso & ")"
        commandes(3) = "B<-convert.list2tab(A)"

        commandes(4) = "write.table(B$" & liste_tables(0) & ", file ='" & fichier & "',col.names=FALSE,row.names=FALSE, sep = '\t',quote=FALSE);" & "cat('-----\n', file ='" & fichier & "',append=TRUE);"
        For compteur_commandes As Integer = 1 To 9
            commandes(compteur_commandes + 4) = "write.table(B$" & liste_tables(compteur_commandes) & ", file ='" & fichier & "', col.names=FALSE,row.names=FALSE,sep = '\t',quote=FALSE,append=TRUE);" & "cat('-----\n', file ='" & fichier & "',append=TRUE);"
        Next
        commandes(14) = ""
        commandes(15) = ""
        commandes(16) = ""

        commandes(17) = "pdf(file='" & Replace(fichierpdf, "\", "\\") & "')"
        'commandes(18) = "plot_ETdiagnosis(A)"
        commandes(18) = "plot(A" & log_ech_diag & ")"
        commandes(19) = ""
        If Not same_mf.Checked Then
            'commandes(19) = "B<-plot_ETdiagnosis_isopleth(A)"
            commandes(19) = "B<-plot_ETdiagnosis_isopleth(A)"
            commandes(20) = "for (pecheries in names(B)) {write.table(B[[pecheries]], file ='" & fichier & "', sep = '\t',append=TRUE,quote=FALSE);cat('-----\n', file ='" & fichier & "',append=TRUE)}"
        Else
            If All_group.Checked Then
                commandes(21) = "A<-E_MSY_0.1(ETM" & param_EMSY & ")"
                commandes(22) = "write.table(A, file ='" & fichier & "', sep = '\t',quote=FALSE,append=TRUE);" & "cat('-----\n', file ='" & fichier & "',append=TRUE);"
                commandes(23) = "par(mar=c(5,4,1,8));plot(row.names(A),A[,'E_0.1'],ylim=range(range(A[,'E_0.1'],na.rm=T,finite=T),range(A[,'E_MSY'],na.rm=T,finite=T),na.rm=T,finite=T),type='l',lwd=2,col='blue',xlab='Trophic levels',ylab='E');abline(h = 1)"
                commandes(24) = "lines(row.names(A),A[,'E_MSY'],type='l',lwd=2,col='red')"
                commandes(25) = "legend(6,range(range(A[,'E_0.1'],na.rm=T,finite=T),range(A[,'E_MSY'],na.rm=T,finite=T),na.rm=T,finite=T)[2],legend=c('E_MSY','E_0.1'),lty=c(1,1),col=c('red','blue'),xpd=NA)"

                commandes(26) = "plot(row.names(A),A[,'F_0.1'],xlim=c(2,5.5),ylim=range(range(A[,'F_0.1'],na.rm=T,finite=T),range(A[,'F_MSY'],na.rm=T,finite=T),na.rm=T,finite=T),type='l',lwd=2,col='blue',xlab='Trophic levels',ylab='F') "

                commandes(27) = "lines(row.names(A),A[,'F_MSY'],type='l',lwd=2,col='red')"
                commandes(28) = "legend(6,range(range(A[,'F_0.1'],na.rm=T,finite=T),range(A[,'F_MSY'],na.rm=T,finite=T),na.rm=T,finite=T)[2],legend=c('F_MSY','F_0.1'),lty=c(1,1),col=c('red','blue'),xpd=NA)"

            End If
        End If



        commandes(29) = "dev.off()"
        commandes(30) = " quit('yes')"

        'on execute ce code R
        Try
            Dim output2() As String = Nothing
            execute_r(commandes, output2)

        Catch ex As Exception
            'MessageBox.Show("Problem in R script: " & ex.Message)
            cLog.Write(ex, "frmEcoTroph.Button4_Click_2")
        End Try



        result_pdf_et_diag.Navigate(fichierpdf)

        If My.Computer.FileSystem.FileExists(fichier) Then


            Dim recup() As String = File.ReadAllLines(fichier)


            Dim totales As String = Join(recup, vbNewLine)
            Dim matrices() As String = Split(totales, "-----")
            Try

                charge_grid(matrices(0).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), grille_ET_main_diagnose)
                charge_grid(matrices(1).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_B)
                charge_grid(matrices(2).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_B_acc)
                charge_grid(matrices(3).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_FL_P)
                charge_grid(matrices(4).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_FL_P_acc)
                charge_grid(matrices(5).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_Kin)
                charge_grid(matrices(6).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_Kin_acc)
                charge_grid(matrices(7).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_F)
                charge_grid(matrices(8).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_F_acc)
                charge_grid(matrices(9).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_D_Y)
            Catch ex As Exception
                Me.NotifyUser(cStringUtils.Localize("Problem in reading R script output. {0}", ex.Message), eMessageImportance.Critical)
            End Try

            'Chargement des résultats du plot_ETdianosis_ispoleth

            Dim isopleth_output() As String = {"TOT_biomass", "TOT_biomass_acc", "TOT_P", "TOT_P_acc", "Y", "Y_fleet1", "Y_fleet2", "TL_TOT_biomass", "TL_TOT_biomass_acc", "TL_Catches", "TL_Catches_fleet1", "TL_Catches_fleet2"}

            If Not same_mf.Checked Then

                For compteur_output As Integer = 0 To isopleth_output.Length - 1


                    Dim ctrl() As Control = panel_result_diag.Controls.Find(isopleth_output(compteur_output), True)

                    If ctrl.Length = 0 Then

                        Dim myTabPage As New TabPage()
                        myTabPage.Text = isopleth_output(compteur_output)
                        myTabPage.Name = "Tab" & isopleth_output(compteur_output)
                        panel_result_diag.TabPages.Add(myTabPage)
                        Dim dtg As New DataGridView
                        dtg.Name = isopleth_output(compteur_output)
                        dtg.Height = 391
                        dtg.Width = 782
                        dtg.Top = 6
                        dtg.Left = 3
                        dtg.Dock = DockStyle.Fill
                        panel_result_diag.TabPages(panel_result_diag.TabCount - 1).Controls.Add(dtg)
                        charge_grid(matrices(compteur_output + 10).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), dtg)
                    Else
                        charge_grid(matrices(compteur_output + 10).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ctrl(0))
                    End If


                Next

            Else
                'process of deleting isopleth out if they were created before
                For compteur_output As Integer = 0 To isopleth_output.Length - 1


                    Dim ctrl() As Control = panel_result_diag.Controls.Find(isopleth_output(compteur_output), True)

                    If ctrl.Length > 0 Then
                        ctrl(0).Enabled = False
                        'A voir pour éffacer carrément les tables 
                    End If
                Next


            End If
            If (same_mf.Checked And All_group.Checked) Then
                charge_grid(matrices(10).Split(New Char() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries), ET_M_EMSY)
            Else
                ET_M_EMSY.RowCount = 1

            End If

        Else
            Me.NotifyUser(My.Resources.NO_OUTPUT_R, eMessageImportance.Critical)
        End If

        Cursor.Current = Cursors.Default
    End Sub

    Private Sub reset_param_diag_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles reset_param_diag.Click
        TopD.Text = "0.2"
        formd.Text = "0.5"
        beta.Text = "0.1"

        Kfeed.Text = "05"
        Ponto.Text = "0.3"
        same_mf.Checked = True
        Forag.Checked = True


        b_input_check.Checked = False


    End Sub

    Private Sub Forag_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Forag.CheckedChanged
        If Forag.Checked Then
            Kfeed.Enabled = True
            Ponto.Enabled = True
            b_input_check.Checked = False
        Else
            Kfeed.Enabled = False
            Ponto.Enabled = False
        End If
    End Sub

    Private Sub group_param_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub


    Private Sub List_fleet1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles List_fleet1.SelectedIndexChanged

        Dim compteur_fin As Integer = List_fleet1.SelectedItems.Count
        If compteur_fin = List_fleet1.Items.Count Then
            Me.NotifyUser(My.Resources.TOO_SELECTED_FLEET, eMessageImportance.Warning)
        End If

    End Sub



    Private Sub mull_eff_EMSY_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub same_mf_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles same_mf.CheckedChanged
        If Not same_mf.Checked Then
            List_fleet1.Enabled = True

            Dim compteur As Integer

            If (ETinputdata.NumFleet < 2) Then
                Me.NotifyUser(My.Resources.NOT_ENOUGH_FLEET, eMessageImportance.Warning)
                same_mf.Checked = True
                List_fleet1.Enabled = False
            Else


                If (ETinputdata.NumFleet > 1 And List_fleet1.Items.Count = 0) Then


                    For compteur = 0 To ETinputdata.NumFleet - 1
                        List_fleet1.Items.Add(ETinputdata.FleetName(compteur))

                    Next
                End If
            End If

        Else
            List_fleet1.Enabled = False
        End If


    End Sub

    Private Sub Label19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub



    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click



        Try
            Dim myservice As New cEcoBaseWDSL()
            Dim myresult As String
            Dim myresult_xml As New XmlDocument()

            Cursor.Current = Cursors.WaitCursor
            panel_webservi.Visible = True
            panel_webservi.BringToFront()


            If models_list.Items.Count = 0 Then


                Try

                    myresult = myservice.list_models("", Nothing)
                    myresult_xml.LoadXml(myresult)

                    Dim nodelist As XmlNodeList = myresult_xml.DocumentElement.ChildNodes
                Catch ex As Exception
                    Me.NotifyUser(My.Resources.NO_DB_SERVICES, eMessageImportance.Critical)
                    cLog.Write(ex, "Button7_Click")
                End Try


                ReDim num_model(myresult_xml.GetElementsByTagName("element").Count)
                Dim compteur As Integer = 0

                For Each node As XmlNode In myresult_xml.GetElementsByTagName("element")

                    If Not (IsNothing(node("model_name"))) Then


                        models_list.Items.Add(node("model_name").InnerText)
                        num_model(compteur) = node("model_number").InnerText
                        compteur = compteur + 1
                    End If
                Next

            End If
            Cursor.Current = Cursors.Default
        Catch ex As Exception
            cLog.Write(ex, "Ecotroph::Button7-Click")
            Me.NotifyUser(My.Resources.ERROR_NO_WS, eMessageImportance.Critical)
        End Try

    End Sub

    Private Sub models_list_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles models_list.DoubleClick

        Dim myStream As Stream = Nothing
        Dim openFileDialog1 As New OpenFileDialog()
        panel_webservi.Visible = False


        openFileDialog1.InitialDirectory = "c:\"
        openFileDialog1.Filter = My.Resources.FILEFILTER_XML
        openFileDialog1.FilterIndex = 2
        openFileDialog1.RestoreDirectory = True



        Dim url_eco As String

        url_eco = "http://ecobase.ecopath.org/php/extract_model.php?model=" & num_model(models_list.SelectedIndex)


        Try
            Dim myservice As New cEcoBaseWDSL()

            ' Jerome refonte et utilisation webservice 13/12/2012

            'Dim myresult As String
            'Dim myresult_xml As New XmlDocument()



            ' Good way to get the model from the webservice

            'myresult = myservice.getModel("input_data", num_model(models_list.SelectedIndex))
            'myresult_xml.LoadXml(myresult)

            ' old way to do the same think without webservices

            'Cela devrait être fait avec le myservice getmodel mais les 2 extract_model et get_model ne renvoie pas la même chose
            'et ans le second cas ce qui est renvoyé n'ets pas apprécié. Il faut modifier le web service pour qu'il soit plsu conforme
            'ce qui ne doit pas être grand chose


            'Dim Str As System.IO.Stream
            'Dim srRead As System.IO.StreamReader

            'Try
            ' make a Web request
            'Dim req As System.Net.WebRequest = System.Net.WebRequest.Create(url_eco)
            'Dim resp As System.Net.WebResponse = req.GetResponse
            'Str = resp.GetResponseStream
            'srRead = New System.IO.StreamReader(Str)
            'myresult = srRead.ReadToEnd()


            'myresult_xml.LoadXml(myresult)

            'Catch ex As Exception

            'Finally
            '  Close Stream and StreamReader when done
            '   srRead.Close()
            '  Str.Close()
            'End Try

            'Dim fichier_data_transfert As String = cFileUtils.MakeTempFile(".xml")
            'myresult_xml.Save(fichier_data_transfert)

            'Dim file As New System.IO.StreamReader(fichier_data_transfert)

            Dim reader As New System.Xml.Serialization.XmlSerializer(GetType(ETinputtot))




            ETinputdata = CType(reader.Deserialize(New StringReader(myservice.getModel("input_data", num_model(models_list.SelectedIndex)))), ETinputtot)



            Dim DataGrid As DataGridView = Me.ETgridinput
            'List faut une procédure pour afficher cela
            For igrp As Integer = 0 To ETinputdata.TL.Length - 2
                If (DataGrid.RowCount < ETinputdata.TL.Length) Then
                    DataGrid.Rows.Add()
                End If

                DataGrid.Item(0, igrp).Value() = ETinputdata.GroupName(igrp + 1)
                DataGrid.Item(1, igrp).Value() = ETinputdata.TL(igrp + 1)
                DataGrid.Item(2, igrp).Value() = ETinputdata.B(igrp + 1)
                DataGrid.Item(3, igrp).Value() = ETinputdata.PROD(igrp + 1)

                If Not (IsNothing(ETinputdata.accessibility)) Then DataGrid.Item(4, igrp).Value() = ETinputdata.accessibility(igrp + 1)
                If Not (IsNothing(ETinputdata.OI)) Then DataGrid.Item(5, igrp).Value() = ETinputdata.OI(igrp + 1)

            Next
            If Not (IsNothing(ETinputdata.Comments)) Then commentaires.Text = ETinputdata.Comments Else commentaires.Text = ""
            If Not (IsNothing(ETinputdata.ModelName)) Then Modelname.Text = ETinputdata.ModelName Else Modelname.Text = ""
            If Not (IsNothing(ETinputdata.ModelDescription)) Then modeldescription.Text = ETinputdata.ModelDescription Else modeldescription.Text = ""
            DataGrid.ColumnCount = 6 + ETinputdata.NumFleet
            For ifleet As Integer = 0 To ETinputdata.NumFleet - 1
                DataGrid.Columns(6 + ifleet).Name = ETinputdata.FleetName(ifleet)
                For igrp As Integer = 0 To ETinputdata.TL.Length - 2
                    DataGrid.Item(6 + ifleet, igrp).Value() = ETinputdata.Catches(ifleet)(igrp + 1)
                Next

            Next
            DataGrid.Columns(4).DefaultCellStyle.BackColor = Drawing.Color.BurlyWood
            Button2.Enabled = True
            Button3.Enabled = True
            Button4.Enabled = True

        Catch Ex As Exception
            cLog.Write(Ex, "Ecotroph::models_list")
            Me.NotifyUser(cStringUtils.Localize(My.Resources.NO_MODEL_DATA, Ex.Message), eMessageImportance.Critical)
        Finally
            ' Check this again, since we need to make sure we didn't throw an exception on open.
            If (myStream IsNot Nothing) Then
                myStream.Close()
            End If
        End Try

    End Sub

    Private Sub models_list_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles models_list.SelectedIndexChanged

        Dim url_eco As String
        url_eco = "http://sirs.agrocampus-ouest.fr/EcoTroph/index.php?ident=base_eco&pass=base_eco&provenance=ecopath&action=base&menu=0&model=" & num_model(models_list.SelectedIndex)
        site_eco.Navigate(New Uri(url_eco))

    End Sub

    Private Sub All_group_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles All_group.CheckedChanged
        If All_group.Checked Then
            list_group_diag.Enabled = False

        Else
            list_group_diag.Enabled = True
            Dim compteur As Integer


            If (ETgridinput.RowCount > 1 And list_group_diag.Items.Count = 0) Then


                For compteur = 1 To ETgridinput.RowCount - 2
                    If (DirectCast(ETgridinput.Item(4, compteur).Value, Single) > 0) Then list_group_diag.Items.Add(ETgridinput.Item(0, compteur).Value)

                Next
            End If


        End If
    End Sub

    Private Sub list_group_diag_old_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub list_group_diag_SelectedIndexChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles list_group_diag.SelectedIndexChanged

    End Sub

    Private Sub Log_scale_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Log_scale.CheckedChanged

    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        panel_webservi.Visible = False


    End Sub







    Private Sub PictureBox3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox3.Click
        System.Diagnostics.Process.Start(aide & "#smooth1")
    End Sub

    Private Sub PictureBox4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox4.Click
        System.Diagnostics.Process.Start(aide & "#transpose")
    End Sub

    Private Sub PictureBox5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox5.Click
        System.Diagnostics.Process.Start(aide & "#diagnose")
    End Sub


    Private Sub result_pdf_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles result_pdf.DocumentCompleted

    End Sub

    Private Sub frmEcotroph_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub TopD_MaskInputRejected(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MaskInputRejectedEventArgs) Handles TopD.MaskInputRejected

    End Sub

    Private Sub ETgridinput_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles ETgridinput.CellContentClick

    End Sub

    Private Sub TabPage1_Click(sender As System.Object, e As System.EventArgs) Handles TabPage1.Click

    End Sub

    Private Sub NotifyUser(strMessage As String, status As eMessageImportance, Optional strLink As String = "")

        Dim msg As New cMessage(strMessage, eMessageType.Any, eCoreComponentType.External, status)
        msg.Hyperlink = strLink
        Try
            Me.Core.Messages.SendMessage(msg)
        Catch ex As Exception
            cLog.Write(ex, "frmEcoTroph.NotifyUser(" & strMessage & ")")
        End Try

    End Sub

    Private Function AskUser(strMessage As String, style As eMessageReplyStyle, Optional status As eMessageImportance = eMessageImportance.Question) As eMessageReply

        Dim msg As New cFeedbackMessage(strMessage, eCoreComponentType.External, eMessageType.Any, status, style)
        Try
            Me.Core.Messages.SendMessage(msg)
        Catch ex As Exception
            cLog.Write(ex, "frmEcoTroph.AskUser(" & strMessage & ")")
        End Try
        Return msg.Reply

    End Function

End Class