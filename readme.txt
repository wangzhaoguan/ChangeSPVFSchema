����������⡿��
	���ݿ�ָ�����ͬʵ�����󣬹�Ӧ���Ĳ��ֹ��ܣ�ʹ�ó�����ʾ�Ҳ��� ԭ��9999�û��ĺ�����
        ���£������Զ��庯������ͼ����������ǿ����schema�����

�������衿��
	1����д���ݿ�������Ϣ�����������ӡ�
	2����дԭ������(ע������ԭ����������ĸ�Ĵ�Сд)
	3�������޸�����
	4����������鿴��ʾ���ļ���C:\GSLog\GS_ChangeSchema.txt
	5���ֹ��޸���ʾ����Ĵ洢���̣����´�������Ĵ洢���̡�
	6�������滻�����������������滻��ϡ�
	��ʾ2012-6-24 15:08:46 :���ϴ洢�����е�������LC####9999����ΪLC****9999���Ѹ�����ϡ�	
	7������IIS 
 
��ѯSQL���£�
select t.name SPName, t.type, s.definition SPDef
	, case 
		when type = 'FN' then 1 
		when type = 'TF' then 1 
		when type = 'V'  then 2 
		when type = 'P'  then 3 
		else 99 end 	as idx 
from sys.objects t 
	join sys.sql_modules s on t.object_id = s.object_id 
where t.type IN ('FN', 'TF', 'P', 'V', 'TR') 
 and s.definition like '%LCXXX9999.%'  --oldUser
order by idx, t.type
 
---------------------------------
updated 2018-08-01
����exe�Ĺ�������ͬ�ģ�һ������Ϊ.NET 2.0����һ��Ϊ.NET 4.0
��ҪΪ�˽������win10��2012���ϰ汾��OS��ʾ��װ.NET 3.5������