
#### Usage
* direct download - import ScssParser.dll to your project

## Simple Code

```c#
var scss = new ScssParser.Parser().ReadFileText(@"
$bg:red;

html {
  box-sizing: border-box;
  -webkit-font-smoothing: antialiased;
}

body {
  min-height: 100vh;
  display: flex;
  font-family: ""Inter"", Arial;
  justify-content: center;
  align-items: center;
  background: $bg;
  color: #000;
  .socials {
    position: fixed;
    display: flex;
    right: 20px;
    bottom: 20px;
    > a {
      display: block;
      height: 28px;
      margin-left: 15px;
      &.dribbble img {
        height: 28px;
      }
      &.twitter svg {
        width: 32px;
        height: 32px;
        fill: #1da1f2;
      }
    }
  }
  .grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    grid-gap: 1rem;
  }
}
");
```
