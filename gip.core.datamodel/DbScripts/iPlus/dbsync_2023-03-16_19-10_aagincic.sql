delete vb
from VBGroupRight vb
inner join ACClassProperty pr on pr.ACClassPropertyID = vb.ACClassPropertyID
inner join ACClass cl on cl.ACClassID = pr.ACClassID
where
	cl.ACIdentifier = 'ACClass'
	and pr.ACIdentifier = 'IsSelected'

delete pr
from ACClassProperty pr
inner join ACClass cl on cl.ACClassID = pr.ACClassID
where
	cl.ACIdentifier = 'ACClass'
	and pr.ACIdentifier = 'IsSelected'