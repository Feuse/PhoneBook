# PhoneBook

All the data for the phone book should be held in a single file.
Adding or updating a new entry must not overwrite the entire file. 
Aside from the filename, this class cannot have any fields or members. 

I used Indexing to have control over where the data is at any give time.

indexing by entry name and providing another field called "Blocks" which are two numbers coresponding to the byte location in the file.
using a filestream to navigate through the file with the index.

examples-

ADDING:

ADD ---> {name: roman, phone: 0536201491} -----> roman = index in bytes, block = [0 - bytes.len]

ADD ---> {name: oren, phone: 050111000}  ---->   oren = index in bytes, block = [last block - bytes.len]

UPDATING:

{name: roman, phone: 0536201491}   <-------UPDATE TO -------> {name: roman, phone: 1}  (shorter)

{name: oren, phone: 050111000} 

when shorter(less bytes) decrease block size by ammount that got shorted, on all blocks, same goes for updating bigger(more bytes).
