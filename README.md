# TimeTravelHack

(scroll down for instructions on how to run and use the app)

Hack Manchester 2018, the TimeTravel team: Luce Carter, Cynthia Lee, Sal Freudenberg, Clare Sudbery.

For Hack Manchester 2018 (https://www.hac100.com/event/hack-mcr-18/), we built a team machine!
(We also won Best in Show for our efforts. Out of 57 teams. Just sayin').

Our time machine is a deliberately ridiculous concept that allows people to click a button whenever they need more time in their day. Click the button, wait a specified time period, have a nap… when you’re done, the app will wake you up and reset your clock to whatever time it was when you pressed the button. So you can fall asleep at 3pm for twenty minutes, then wake up and it’s still 3pm.

Not only does it reset your time, it resets everybody’s time… by texting Greenwich and asking them to reset the clock. We thought it would be particularly useful if you were on a train running late: Click your button, enter the time of the delay (eg 30 mins), then in 30 mins, the app resets the clock and your train isn’t late any more!

We’ll conveniently ignore all the people standing on platforms waiting for trains that never arrive because the clocks keep winding back. Also the more people that use the app, the longer everybody’s day gets. In theory your day could never end… and the sun would start rising and setting at really odd times. Although there is some fun maths around the fact that if one person asks for time before the end of a previous user’s request, the day doesn’t lengthen by as much as you might think (see TimeTravelHack/TimeTravelApi/tests/MoreTimeRequestControllerTests.cs).

We wrote more about our experiences here:  
Clare: https://medium.com/a-woman-in-technology/hack-manchester-2018-best-in-show-ca6ef65fb49c  
Sal: https://salfreudenberg.wordpress.com/2018/11/07/hack-like-a-woman-how-self-care-and-some-agile-techniques-helped-us-win-hackathon-best-in-show/  
Cynthia: https://cynthialee.xyz/hackmcr2018  
Luce: https://lucecarter.co.uk/we-only-went-and-won-hack-manchester-2018/

We made a video about it here: https://youtu.be/EjgwNEOZt8w

# TO RUN THE CODE:
1) Install .Net Core - it will work on either Mac, Linux or Windows: https://www.microsoft.com/net/download
2) Clone the TimeTravelHack repo (this repo). How to clone a repo: https://help.github.com/articles/cloning-a-repository/
3) Navigate to the TimeTravelApi folder in a command prompt, and run the following command: dotnet run
4) Visit the following url into your browser: http://localhost:nnnn, but replace nnnn with the port number. The default is 5000 or 5001, otherwise check the output on the command line, and it will tell you which port the software is listening on.
5) See instructions below on using the app.

# TO TEST THE CODE:
Navigate to the TimeTravelApi folder in a command prompt, and run the following command: dotnet test
If you want to add tests to your .Net Core project, you might find this blog post useful: https://insimpleterms.blog/2018/10/31/adding-nunit-tests-to-a-net-core-console-app/

# TO USE THE APP:
The app has been designed to work with a puck.js IoT button (https://www.puck-js.com/), but it will still work if you don't have one.
## IF YOU HAVE A PUCK.JS BUTTON:
1) Click the tardis.
2) Select the puck you want to pair with (you might have to wait a few seconds while it searches for your puck).
3) This should have automatically created a time request. 
4) To create a new request, click your Puck.js button.
5) See "WHETHER YOU HAVE A BUTTON OR NOT" below.
## IF YOU DON'T HAVE A PUCK.JS BUTTON:
1) Click the tardis.
2) Click Cancel on the pairing dialog.
3) Click OK when asked if you would like to click on the tardis to create new requests.
4) This should have automatically created a time request. 
5) To create a new request, click the tardis again.
6) See "WHETHER YOU HAVE A BUTTON OR NOT" below.
## WHETHER YOU HAVE A BUTTON OR NOT:
- The default request length is 20 minutes.
- If you want to change the request length, change the number below the tardis but DON'T click Enter (or not at the time I wrote this, at any rate).
- Each new request will appear in the grid at the bottom of the page.
- When a request expires, at the end of the relevant minute, your puck.js button will flash and the heading in the browser will flash, and the clock will be reset (for ALL users) by the relevant amount.
- If you close down a browser tab then any connected requests will never get marked as "alerted", but they will still expire.
- If you add a new request from the same browser tab when a previous request from the same tab hadn't expired, the previous request will be cancelled (it will be marked as Alerted and Expired).
- You can see other user's requests as well, but other requests are only added when your display refreshes every minute.
- If you want multiple users, open up multiple browser tabs all connected to the same url. Each browser tab can be connected to a different puck.js button, or they can all be connected to the same button (in which case one puck click will generate multiple requests), or they can all be puckless, or you can mix and match.
- If you want to know what happens when you have multiple overlapping requests from different users, check out all the relevant tests in TimeTravelHack/TimeTravelApi/tests/MoreTimeRequestControllerTests.cs.

