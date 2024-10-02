## Notes

Local details were placed as comments in class files where they make more sense

With few exceptions, tests are mostly comment-free, where test method names and the code itself should clearly express the intent

This document decribes overall approach.

Patch diff results can be expressed in many human-readable ways. 
After reading the prompt and looking at provided patch samples I made the following decisions:

Key+BeginDate+EndDate combination will be used as a "master key"" to identify existing/new/removed lines

Based on provided samples I found that the same master key may appear more than once with different values.
I'm combining such real lines from the file into one "logical" line. It's happening from top to bottom,
so non-emplty values appearing later in the file overwrite earlier values for the same master key.

If the set of value columns are different between old and new patch, I'm either adding such columns or
removing them to facilitate future comparison. Added columns do not require any special treatment, 
removed columns will be reported.

Summary would contain up to four sections:

1. Information about fields/columns that were deleted in a new patch compared to the old one.
It means that the overrides were removed for the whole file, all master keys.

2. Information about NEW overrides, e.g. for master keys not seen in the "old" file

3. Information about REMOVED overrides, e.g. for master keys that were in the "old" file, but
no longer exist in a "new" file

4. Information about CHANGED overrides, e.g. same master key exists in both patches, but we detected some 
changes in the values

In addition to command line arguments that existed in provided template, I added optional parameter -e
If the app is called with that parameter, it will write the output to some file in system's temp folder
and would open it in a text editor. 

Some code and patterns in this excercise may bee seen as redundant/verbose. However, I did not try to 
write as few lines of code as possible. Instead I wrote it in a way I would approach a new app that
may have simple requirements right now but later will likely be extended. 