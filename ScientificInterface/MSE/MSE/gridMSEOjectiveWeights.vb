#Region " Imports "

Option Strict On
Option Explicit On

Imports EwECore
Imports EwECore.SearchObjectives
Imports EwEUtils.Core

#End Region

''' ===========================================================================
''' <summary>
''' gridMSEOjectiveWeights provides a wrapper around gridSearchObjectivesGroup,
''' for the MSE, so it has a constructor with no arguments and can be created 
''' by the NavigationPanel.
''' </summary>
'''<remarks>WARNING 8-march-2010 Objective weights are not being use by the MSE at this time. The grid has been left in place(for now) incase they are to be re-implemented </remarks>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridMSEOjectiveWeights
    : Inherits Ecosim.gridSearchObjectivesWeight

    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Property UIContext() As cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As cUIContext)
            If value IsNot Nothing Then
                Me.Manager = value.Core.FishingPolicyManager
            End If
            MyBase.UIContext = value
        End Set
    End Property

End Class
