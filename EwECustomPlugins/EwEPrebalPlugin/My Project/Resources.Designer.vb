﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("EwEPrebalPlugin.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Pre-balance diagostics.
        '''</summary>
        Friend ReadOnly Property HEADER_PLOT() As String
            Get
                Return ResourceManager.GetString("HEADER_PLOT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Entered.
        '''</summary>
        Friend ReadOnly Property LABEL_ENTERED() As String
            Get
                Return ResourceManager.GetString("LABEL_ENTERED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Estimated.
        '''</summary>
        Friend ReadOnly Property LABEL_ESTIMATED() As String
            Get
                Return ResourceManager.GetString("LABEL_ESTIMATED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Regression.
        '''</summary>
        Friend ReadOnly Property LABEL_REGRESSION() As String
            Get
                Return ResourceManager.GetString("LABEL_REGRESSION", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Regression f(x)={0}x{1}{2}.
        '''</summary>
        Friend ReadOnly Property LABEL_REGRESSION_FORMULA() As String
            Get
                Return ResourceManager.GetString("LABEL_REGRESSION_FORMULA", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Group (high to low Trophic Level).
        '''</summary>
        Friend ReadOnly Property LABEL_XAXIS() As String
            Get
                Return ResourceManager.GetString("LABEL_XAXIS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Show group names.
        '''</summary>
        Friend ReadOnly Property OPTION_SHOWNAME() As String
            Get
                Return ResourceManager.GetString("OPTION_SHOWNAME", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Show regression formula.
        '''</summary>
        Friend ReadOnly Property OPTION_SHOWREGFORMULA() As String
            Get
                Return ResourceManager.GetString("OPTION_SHOWREGFORMULA", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Show trophic levels.
        '''</summary>
        Friend ReadOnly Property OPTION_SHOWTL() As String
            Get
                Return ResourceManager.GetString("OPTION_SHOWTL", resourceCulture)
            End Get
        End Property
    End Module
End Namespace