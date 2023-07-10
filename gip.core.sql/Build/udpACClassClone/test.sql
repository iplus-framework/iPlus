select 
pr.ACProjectName,
cl.ACClassID,
cl.ACIdentifier
from ACClass cl
inner join ACProject pr on pr.ACProjectID = cl.ACProjectID
where cl.ACIdentifier like '%ACMaintService%'

