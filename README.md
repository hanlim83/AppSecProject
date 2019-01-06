# ASPJ
A Project for Application Security & Project (ITP292) Module 
## Team Members
* [Evelyn Lim](https://github.com/elxwy)
* [Hansen Lim](https://github.com/hanlim83)
* [Hugo Chia](https://github.com/Kool-Koder)
* [Karen Ong](https://github.com/karen620)
* [Zhi Sheng](https://github.com/Yakzhisheng)

## Summary

>Hacking competitions such as Capture the Flag used to be few and far between. Capture the Flag is a series of security challenges, from Cryptography to Web Exploits, Reverse-engineering and more. Participants submit a flag (a answer they found) once they solve the challenge.

However, it is gaining more popularity in recent years, and we hope to make it easier to administer challenges (as it provides another view, compared to just solving the challenges) and for the community to learn from each other. It aims to reduce the amount of administrative work to set up the challenge environment and to focus more on creating the challenges. This is also an integrated all-in-one platform whereby users can go to the forum or chat inside this platform, instead of having to use multiple platforms for a competition.

## Features
- CTF Administration
  - Administer challenges (Set questions, points, upload files, etc.)
  - Flag submission and validation
  - Scoreboarding
  - Team/Participant Result Analysis (For educational purpose)
  - Extra security logging to capture data needed to detects attacks on the game-system
- Forum
  - To be able to review and view topics posted by other users, when it is posted. It is like a sharing discussion board of authorized users to help each other with their challenge. 
  - To be able to learn together
  - Before users are able to post a link, we will check if they are posting any malicious websites, so system can decide whether to send a warning or just block it.
- Platform Management
  - Servers required for challenges can be created from scratch or via a previously created server image
  - Management of the challenge servers can be done via the platform
  - Analytics on the performance of the servers and the platform are available directly within the platform
- News Feed 
  - Only visible for Administrators.
  - To be able to view recent security-related news and use that as a reference for challenges towards users.
  - Administrators are also able to search the news with related tags.
- Live Chat
  - User are able to message within a group.
  - They are also able to privately message someone or administrator.
  - User will be able to immediately receive the message once send out.

## External Resources Used
- Bootstrap (Local JS & CSS when in Development, CDN when in Production)
- jQuery (Local JS when in Development, CDN when in Production)
- jQuery Vaildation (Local JS when in Development, CDN when in Production)
- jQuery Vaildation Unobtrusive (Local JS when in Development, CDN when in Production)
- Popper (Local JS when in Development, CDN when in Production)
- Google Material Icons (CDN regardless of Development or Production)
- TurboLinksJS (Local JS when in Development, CDN when in Production)
- Progress Tracker (Local CSS regardless of Development or Production)
- DataTables (Local JS & CSS when in Development, CDN when in Production)

## Tools Used
- Visual Studio 2017
- ASP.NET Core 2.1 Framework
- ASP.NET Entity Core Framework
- Amazon Web Services (S3, RDS, EC2, EBS, VPC, SNS, ELB)
