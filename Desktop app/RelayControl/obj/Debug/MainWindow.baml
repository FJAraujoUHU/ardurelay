   M S B A M L   `   `   `  ����G  CRelayControl, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:RelayControlRelayControl  5   	       RelayControl.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:RelayControl     5   	     � Title$  Relay Controller��5   	   $	��640��6"   $	��440��6/   $	��440��5	   F   $	��200��6U    � Icon$ /arduino.png��6e    � DataContext 6	   ��  �� RelativeSource �� *Self+.��5      ]� $��10q�6   .+�5   
   ]� $	��Top=�6   $��24��6)   .+�5      �� -   5        $��
btnRefresh��5   L   $��Right=�6   $��10,0,0,0q�6,   $��14(�6>   $��Center=�6w   $��Right=�6�   $��23��6�   $��23��6�   $��Center=�6�   $��Center=�6�   .��6  ↻6  �� 5      -   5        $��cmbPorts��5       �� Text$ Select a device...��6G   $��Center=�6�   ��6'   �� *	Devices+6�   5      �� 5   
   $
��Mode��6   $��Bottom=�6!   $��63��69   .��5      ]� .+�5      	� -   5        $��RadON��5   0    	� 	GroupName$ Modes��6   $��Center=�6=   $
��Left=�6_   $��0,0,10,0q�6u    d� 	IsChecked$
 TrueY�6�   .��6�   On6�   	� 5      -   5        $��RadOFF��5   0   $ Modes��6   $��Center=�6>   $
��Left=�6`   $��0,0,10,0q�6v   .��6�   Off6�   	� 5      -   5        $��	RadToggle��5   0   $ Modes��6   $��Center=�6A   $
��Left=�6c   $��0,0,10,0q�6y   .��6�   Toggle6�   	� 5      -   5        $��RadPulse��5   0   $ Modes��6   $��Center=�6@   $
��Left=�6b   $
��Left=�6x   .��5      �� $��Pulse��6   $��Center=�6.   .��5      ��  �� Orientation$ 
Horizontal=�6%   .+�5      �� -   5        $��TxtPulseDuration��5   &   $��Center=�6>   $��42��6Y    �� TextAlignment$ Right=�6�   .��6�   10006�   �� 5      $��Center=�6(   $��2,0,5,0q�6C   ms6X   �� 5      -   5        $��ChkPulseCounting��5   '   $ FalseY�6?   $��Center=�6Q   $��0,0,5,0q�6l   .��6}   Count6�   �� 5      -	   5        $��TxtPulseCount��5   &   $��Center=�6y   $��35��6�   $ Right=�6�   {�6;   �� *	IsChecked+	 �� ElementName$	 ChkPulseCounting��.��6�   56�   5      5      5      �� 5       -
   5        $��btnSend��5       $��Right=�6(   $��Right=�6?   $��0,5,1,5q�6[   {�.  $��36��6~   .��6�   Send6�   5!      5"      �� 5$   
   -   5        $��btnSelectAll��5$      $��14(�6   $��Bottom=�6M   {�.  .��5%      �� $ 
Horizontal=�6   .+�5&      �� ALL6!   �� 5'      -   5        $��btnSelectAllSelected��5'      
�������$��Italic$�6J   $��10,0,0,0q�6]   $y�	Collapsed=�6o   
Selected6�   5(      5)      � 5/   
   -   5        $��	GrdRelays��5/      
 � RowDefinitions
 50       � ColumnDefinitions 52      �� 53      $��1* �6#   �� 54      $��1* �6#   58      59      5:      