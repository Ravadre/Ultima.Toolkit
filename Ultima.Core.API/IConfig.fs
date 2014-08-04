namespace Ultima

type IConfig = 
     abstract GetSection<'T> : string -> 'T

