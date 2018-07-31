' =============================================================================
'
' $Log: ucPSDPlotzgc.vb,v $
' Revision 1.2  2009/02/21 00:23:07  jeroens
' Added headers
'
' =============================================================================

#Region " Imports "

Option Explicit On
Option Strict On

Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Commands
Imports ZedGraph

#End Region

Public Class ucPSDPlotzgc
    Private m_core As cCore = cCore.GetInstance()
    Private m_zgh As ZedGraphHelper = Nothing

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        PopulateGroupBoxes()
        Me.m_zgh = New ZedGraphHelper(Me.zgcZedGraphCntl)

    End Sub

    Private Sub PopulateGroupBoxes()
        llbGroups.SuspendLayout()

        llbGroups.Items.Clear()
        'llbGroups.Items.Add(New LegendListBox.EcopathGroupItem(Nothing))
        For i As Integer = 1 To m_core.nLivingGroups
            llbGroups.Items.Add(New LegendListBox.EcopathGroupItem(m_core.EcoPathGroupInputs(i)))
        Next

        llbGroups.ResumeLayout()
    End Sub
End Class
