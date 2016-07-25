# RDLCReportUpgrade
Tool for transformation of Dynamics NAV reports from Classic to RDLC

Creates dtaaset, transformes the request form into request page (minus code on controls), creates a RDLC layout with header, footer (connected with GetData SetData funstions). Body of RDLC contains only textboxes - tablixes, grouping, visibility, has to be manually done.

RDLCUpgradeTool v1.00.exe - exe file

RDLCUpgradeTool v1.00.zip - project template for visual studio

source folder - same as RDLCUpgradeTool v1.00.zip only unzipped


Input/Output:
- input file is txt file exported from Dynamics NAV with classic reporting (single report)
- output file is txt file that can be imported to Dynamics NAV with RDLC reporting (single report)

Instructions: 
1. Import file - import the input file
2. Decustruct - deconstruct the input file into parts - you can see the values of parts, and their hierarhy below in the bottom part of the form
3. Transform - perform transformations, creates dataset, request page,RDLC layout
4. View sections - opens a new form, where one can see and set what sections will be on header, footer or body. RDLC Layout is recreated once the form is closed.
5. Export File - creates the output file
