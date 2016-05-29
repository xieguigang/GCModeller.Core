# GCModeller.Core
GCModeller base core assembly library on common biological database read and write I/O

The library was public release avaliable at nuget site: https://www.nuget.org/packages/GCModeller.Core/

>  PM>  Install-Package GCModeller.Core

### Library overviews

This project is the core lib of GCModeller, it provides the common components in the GCModeller analysis tools such as sequence model and protein structure models, some necessary interface and component class for build the bio-system model in the GCModeller.

```vb.net
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ContextModel
Imports LANS.SystemsBiology.SequenceModel
```

This Projects includes some common used biological database reader, the database includes:

1. NCBI GenBank database
2. KEGG DBGET API
3. MetaCyc Database 
4. FASTA sequence database

All of these good staff is in the namespace:

```vb.net
LANS.SystemsBiology.Assembly.Bac_sRNA.org
LANS.SystemsBiology.Assembly.DOMINE
LANS.SystemsBiology.Assembly.DOOR
LANS.SystemsBiology.Assembly.KEGG
LANS.SystemsBiology.Assembly.MetaCyc
LANS.SystemsBiology.Assembly.NCBI
LANS.SystemsBiology.Assembly.Uniprot
```

