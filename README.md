# MESWebSample
This source code demo about manage Device/Machine and Plan for it in factory.
A Plan include info: Device name, Worker Name, Target (assign for it) and Counting (current target-number already complete)
Feature:
1. Display list of Machine and status online/offline
2. Display list of: Company, factory, worker.
3. Export Plan to excel file by Today/Yesterday or by Machine online
4. Creat/modify Plan for each Machine by using import excel or modify on website
5. Search Plan by Machine, Worker
6. Display column chart, all Plans will be show with 2 columns Target and Counting.
7. Paging Plan chart, split screen to 2 panels, each panel will have 32 items
8. Update chart by real-time, auto go to next page after 3 seconds
9. If value Counting of a Machine greater than or equal 80% of Target. It will be Red
10. When Machine alert BREAK, display popup status on all browsers, tabs if opening (Display: Machine name, Worker name)
11. When ControlAdmin (an application on outsite) send message, display popup message on all browsers, tabs if opening
