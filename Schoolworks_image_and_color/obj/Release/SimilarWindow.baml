   M S B A M L   `   `   `  ���� V  RSchoolworks_image_and_color, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:Schoolworks_image_and_colorSchoolworks_image_and_color  /    )Schoolworks_image_and_color.SimilarWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ;local)clr-namespace:Schoolworks_image_and_color     -      main_menu_similar  � Title#  �   $��547.434��$
��1282�� � 
ResizeMode$ NoResize=� � 	Resources  �� 
TargetType �� Triggers P� Property P� Value��%8   '��       �� " �� P�  �@J�IsMouseOver . .���  � ColumnDefinitions �� .+�� $��
10,0,10,10q�.+�� $��	0,458,0,0q�$
��Left=�$	��Top=�.+��� -    ��Name$ similar_analyze��  analyze_btn_analyze#���  $
��Left=�$	��Top=�$	��120��$��37��� $��	0,10,0,42q�	 � RowDefinitions	 �� $
��237* ��� $
��256* �.+���   similar_group_complist#���  $��	291,0,0,0q�$��2��.��� $��0,6,0,-2q� �� �� �� �� 	 �� �� .+��� -     similar_btn_image_select#���  $��92��$��32�����	#FFDDDDDD$
��Left=�$	��Top=�$��	0,165,0,0q��� -   #���  $��1��$��92��$��32�����	#FFDDDDDD$
��Left=�$	��Top=�$��	0,165,0,0q��� -   #���  $��2��$��92��$��32�����	#FFDDDDDD$
��Left=�$	��Top=�$��	0,165,0,0q��� -   #���  $��3��$��32�����	#FFDDDDDD$��Center=�$��Center=�$
��Left=�$	��Top=�$��92��$��	0,165,0,0q��� -   #���  $��1��$��92��$��32�����	#FFDDDDDD$
��Left=�$��Bottom=�$��0q��� -   #���  $��1��$��1��$��92��$��32�����	#FFDDDDDD$
��Left=�$��Bottom=�$��0q��� -	   #���  $��2��$��1��$��92��$��32�����	#FFDDDDDD$
��Left=�$��Bottom=�$��0q��� -
   #���  $��3��$��1��$��32�����	#FFDDDDDD$
��Left=�$��Bottom=�$��92��$��0q��� -   
 ��Name$
 similar_comp_img1��$
��Left=�$	��160��$	��Top=�$	��234��$
��Fill=��� -   $
 similar_comp_img2��$��1��$
��Left=�$	��160��$	��Top=�$	��235��$
��Fill=��� -   $
 similar_comp_img3��$��2��$
��Left=�$	��160��$	��Top=�$	��234��$
��Fill=��� -   $
 similar_comp_img4��$��3��$
��Left=�$	��160��$	��Top=�$	��235��$
��Fill=��� -   $
 similar_comp_img5��$
��Left=�$	��160��$��1��$	��Top=�$	��238��$
��Fill=�$��0,11,0,0q��� -   $
 similar_comp_img6��$��1��$
��Left=�$	��160��$��1��$	��Top=�$	��238��$
��Fill=�$��0,11,0,0q��� -   $
 similar_comp_img7��$��2��$
��Left=�$	��160��$��1��$	��Top=�$	��238��$
��Fill=�$��0,11,0,0q��� -   $
 similar_comp_img8��$��3��$
��Left=�$	��160��$��1��$	��Top=�$	��239��$
��Fill=�$��0,11,0,0q��� -    ��Name$ similar_comp_img1_lb��$�� ��$
��Left=�$��
92,165,0,0q�$	��Top=�$��32��$	��146��$��Center=��� -   $ similar_comp_img2_lb��$�� ��$
��Left=�$��
92,165,0,0q�$	��Top=�$��1��$��32��$	��146��$��Center=��� -   $ similar_comp_img3_lb��$�� ��$��2��$
��Left=�$��
92,165,0,0q�$	��Top=�$��32��$	��146��$��Center=��� -   $ similar_comp_img4_lb��$�� ��$��3��$
��Left=�$��
92,165,0,0q�$	��Top=�$��32��$	��147��$��Center=��� -   $ similar_comp_img5_lb��$�� ��$
��Left=�$��
91,177,0,0q�$��1��$	��Top=�$��32��$	��147��$��Center=��� -   $ similar_comp_img6_lb��$�� ��$��1��$
��Left=�$��
92,177,0,0q�$��1��$	��Top=�$��32��$	��146��$��Center=��� -   $ similar_comp_img7_lb��$�� ��$��2��$
��Left=�$��
92,177,0,0q�$��1��$	��Top=�$��32��$	��146��$��Center=��� -   $ similar_comp_img8_lb��$�� ��$��3��$
��Left=�$��
92,177,0,0q�$��1��$	��Top=�$��32��$	��147��$��Center=���   similar_group_target#���  $	��184��$	��286��$
��Left=�$	��Top=�.���� -   $
 similar_origin_image��$	��159��$	��276��$��0q�$
��Fill=���   similar_group_result#���  $
��Left=�$	��184��$��1��$	��Top=�$	��286��$��0,10,0,0q�.���� -   $
 similar_result_image��$	��159��$	��276��$��0q�$
��Fill=��� -   $" similar_btn_target_histogram��  similar_btn_histogram#���  $
��Left=�$��32��$��182,186,0,0q�$	��Top=�$	��104��$��2��{�.  �� -   $" similar_btn_result_histogram��#���  $
��Left=�$��182,194,0,0q�$��1��$	��Top=�$	��104��$��32��{�.  