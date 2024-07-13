# Crystal Frost Internal Design

This is a living document, meaning that it may change, or get out of sync with the source code. These things happen. Changing your mind and doing something different from what was initially designed is a normal part of software development.

In the event that this document and the code disagree, an effort should be made to reconcile that disagreement, because this document is also intended to serve as an introduction to the code for new contributors, and it’s helpful to minimize their confusion.

## Tools and Tech

The selection of Unity as a primary tool for this project implies the use of several other technologies. Unity is tightly coupled with the language C# and the .Net set of technologies. Rather than add complexity by introducing multiple languages and tools, C# should be used as a de facto first choice where appropriate and possible, and steps should be taken to avoid introducing the need for additional tools. The idea is to make getting the source and building as simple as possible, requiring minimal installation and configuration of the developers' computers. 

There are multiple reasons for requiring as little setup and tooling as possible. One reason is to make becoming an contributor as easy as possible. It’s unlikely that this project will be able to hire full time developers for the foreseeable future, thus it will be advantages to rely on people choosing to contribute because they want to. Another reason for avoiding adding lots of tools is that it helps you identify when you are doing something 'The Hard Way'. Unity and Visual Studio are both decades old tools, and are quite comprehensive on their own, so if there is something they don't do for you, its a hint that maybe you don't need to be doing it that way. On other operating systems, VSCode is also a viable option for a editor and debugger for C# code. Hence great care should be taken when considering adding an additional required too.

## Design Goals

As the primary language of the project is C#, and C# is primary and Object-Oriented programming language, it makes sense to embrace known modern best practices for Object-Oriented Programming. At its most basic this means adopting methodologies that manage or reduce complexity, and promote isolation between classes to minimize conflicts between contributors working in different areas of code.

For further reading consider:

* https://en.wikipedia.org/wiki/SOLID
* https://en.wikipedia.org/wiki/GRASP_(object-oriented_design)
* https://en.wikipedia.org/wiki/KISS_principle
* https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it
* https://en.wikipedia.org/wiki/Unit_testing
* https://en.wikipedia.org/wiki/Don%27t_repeat_yourself

## Design Overview

**To Do - New Overview**

I may have barked up the wrong tree entirely here. Where my expectations got a head of my knowledge of what Unity needs. Revision forthcoming

-Kage.

## Philosophical Guidance

Developing software can be a load of fun, but it can also be a major headache. Over time, programmers as a community have found lots of things that work well and lots of things that don't work quite so well, but there's not really any hard and fast rules about what is good and what is bad. Programming is creative work, and what is good can be subjective. Being able to consider things on a case-by-case basis usually gets better results than adopting an Always X and Never Y set of rules. No one is going to be able to give you rules to code by that always work in every situation, but we can have our collective wisdom guide each other. The rest of this section contains nuggets of wisdom that the reader may find helpful.

### Listen to your nose - On code smells

You open up some source and you start looking around, and things look kind of ok, but Its just not pleasant to be there. You could get in, do what you need, and get out, but honestly, you probably should have cleaned up the dead rat behind that pile of boxes that was stinking up the place, if you had known to look behind the boxes. Code smells are like this, something about a place isn't quite right, you aren't sure how it works, or what it’s supposed to do. You don't have to be able to put it in words what the problem is to know something is bad. Our sense of smell is like that, its hard to describe on smell without listing off similar smells, and it can be hard to identify precisely what a smell is, but it’s not so hard to know if its a bad smell.

So you found something that smells bad, now what? If it’s just you, maybe you decide to clean it up. Find what stinks and get rid of it or replace it, or whatever. On a team you might want to find out who knows more about it. There might be a reason it is the way it is. Sometimes things might seem like they smell bad because of a lack of familiarity, and if someone else can explain it to you, and make the smell go away, that’s good, but if you can recognize that something smells because it didn't explain itself, that’s a clue that to make it explain itself better.

You don't have to know precisely what is wrong to know something can be better.





