   M S B A M L   `   `   `  ���� G  CRelayControl, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:RelayControlRelayControl      RelayControl.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:RelayControl       � Title$  Relay Controller��$	��640��$	��440��$	��440��$	��200�� � Icon$ /arduino.png�� � DataContext ��  �� RelativeSource �� *Self+.��]� $��10q�.+�]� $	��Top=�$��24��.+��� -   $��
btnRefresh��$��Right=�$��10,0,0,0q�$��14(�$��Center=�$��Right=�$��23��$��23��$��Center=�$��Center=�.��↻�� -   $��cmbPorts�� �� Text$ Select a device...��$��Center=����� *	Devices+�� $
��Mode��$��Bottom=�$��63��.��]� .+�	� -   $��RadON�� 	� 	GroupName$ Modes��$��Center=�$
��Left=�$��0,0,10,0q� d� 	IsChecked$
 TrueY�.��On	� -   $��RadOFF��$ Modes��$��Center=�$
��Left=�$��0,0,10,0q�.��Off	� -   $��	RadToggle��$ Modes��$��Center=�$
��Left=�$��0,0,10,0q�.��Toggle	� -   $��RadPulse��$ Modes��$��Center=�$
��Left=�$
��Left=�.���� $��Pulse��$��Center=�.����  �� Orientation$ 
Horizontal=�.+��� -   $��TxtPulseDuration��$��Center=�$��42�� �� TextAlignment$ Right=�.��1000�� $��Center=�$��2,0,5,0q�ms�� -   $��ChkPulseCounting��$ FalseY�$��Center=�$��0,0,5,0q�.��Count�� -	   $��TxtPulseCount��$��Center=�$��35��$ Right=�{��� *	IsChecked+	 �� ElementName$	 ChkPulseCounting��.��5�� -
   $��btnSend��$��Right=�$��Right=�$��0,5,1,5q�{�.  $��36��.��Send�� -   $��btnSelectAll��$��14(�$��Bottom=�{�.  .���� $ 
Horizontal=�.+��� ALL�� -   $��btnSelectAllSelected��
�������$��Italic$�$��10,0,0,0q�$y�	Collapsed=�
Selected� -   $��	GrdRelays��
 � RowDefinitions
  � ColumnDefinitions �� $��1* ��� $��1* �