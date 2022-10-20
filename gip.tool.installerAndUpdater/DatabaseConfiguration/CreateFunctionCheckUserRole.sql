IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'iPlusCheckUserRole')
    BEGIN
        DROP  function  dbo.iPlusCheckUserRole
    END

GO
CREATE function dbo.iPlusCheckUserRole() 
returns bit
AS
begin
    declare @roleState bit;
    set @roleState = 0;
    IF IS_SRVROLEMEMBER ('sysadmin') = 1  
		set @roleState = 1;  
	ELSE IF IS_SRVROLEMEMBER ('sysadmin') = 0  
		set @roleState = 0;
	ELSE IF IS_SRVROLEMEMBER ('sysadmin') IS NULL  
		set @roleState = 0;    
    return @roleState;
end