# How to contribute

I'm really glad you're reading this, because I need volunteer developers to help this project come to fruition.


## Testing

There are a handful of unit/integration tests. Please write unit/integration tests examples for new code you create.

## Submitting changes

Please send a [GitHub Pull Request to MedEasy](https://github.com/candoumbe/MedEasy/pull/new/develop) with a clear list of what you've done (read more about [pull requests](http://help.github.com/pull-requests/)).
 When you send a pull request, we will love you forever if you include unit tests as examples. We can always use more test coverage. Please follow our coding conventions (below) and make sure all of your commits are atomic (one feature per commit).

Always write a clear log message for your commits. One-line messages are fine for small changes, but bigger changes should look like this:

    $ git commit -m "A brief summary of the commit
    > 
    > A paragraph describing what changed and its impact."

## Coding conventions

Start reading our code and you'll get the hang of it. We optimize for readability:

  * **Stick to the `*.editorconfig` file** at the root of the repository
  * **Do not `var`** unless there's no other option : `var` should only be used for anonymous types. So instead of `var data = new Something()`, prefer `Something data = new Something()`. Even better, prefer `ISomething data = new Something()` whenever possbile.
  * **Single entry, single exit** : a method should have one entry and one exit. This is just to avoid missing an exit point that could be in the mddle of a complex algorithm.
  I don't really mind the complexity of an algorithm (up to a certain point 😉).

I will always prefer having
```csharp
int result;
if (condition)
   result = 42;
else
   result = 97;
return result;
```
instead of
```csharp
if (condition)
   return 42;
else
   return 97;
```

   The first version is longer for sure, but I'm more comfortable reading a large code block where the exit will always be at the end than having a block of code of the same size without knowing where the exit will be.


  * This is open source software. Consider the people who will read your code, and make it look nice for them. It's sort of like driving a car: Perhaps you love doing donuts when you're alone, but with passengers the goal is to make the ride as smooth as possible.
  * So that we can consistently serve images from the CDN, always use image_path or image_tag when referring to images. Never prepend "/images/" when using image_path or image_tag.

Thanks,
Cyrille NDOUMBE
