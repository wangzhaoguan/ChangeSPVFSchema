【解决的问题】：
	数据库恢复到不同实例名后，供应链的部分功能，使用出错，提示找不到 原有9999用户的函数。
        更新：增加自定义函数、视图、触发器中强命名schema的情况

【处理步骤】：
	1、填写数据库连接信息，并测试连接。
	2、填写原有属主(注意区分原有属主的字母的大小写)
	3、单击修改属主
	4、如果出错，查看提示的文件。C:\GSLog\GS_ChangeSchema.txt
	5、手工修改提示出错的存储过程，重新创建错误的存储过程。
	6、继续替换属主。至所有属主替换完毕。
	提示2012-6-24 15:08:46 :以上存储过程中的属主由LC####9999更改为LC****9999；已更改完毕。	
	7、重启IIS 
 
查询SQL如下：
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
两个exe的功能是相同的，一个编译为.NET 2.0，另一个为.NET 4.0
主要为了解决部分win10、2012以上版本的OS提示安装.NET 3.5的问题