<map version="freeplane 1.3.0">
<!--To view this file, download free mind mapping software Freeplane from http://freeplane.sourceforge.net -->
<node TEXT="Virtual Device Interface" ID="ID_1723255651" CREATED="1283093380553" MODIFIED="1534417822233"><hook NAME="MapStyle">

<map_styles>
<stylenode LOCALIZED_TEXT="styles.root_node">
<stylenode LOCALIZED_TEXT="styles.predefined" POSITION="right">
<stylenode LOCALIZED_TEXT="default" MAX_WIDTH="600" COLOR="#000000" STYLE="as_parent">
<font NAME="SansSerif" SIZE="10" BOLD="false" ITALIC="false"/>
</stylenode>
<stylenode LOCALIZED_TEXT="defaultstyle.details"/>
<stylenode LOCALIZED_TEXT="defaultstyle.note"/>
<stylenode LOCALIZED_TEXT="defaultstyle.floating">
<edge STYLE="hide_edge"/>
<cloud COLOR="#f0f0f0" SHAPE="ROUND_RECT"/>
</stylenode>
</stylenode>
<stylenode LOCALIZED_TEXT="styles.user-defined" POSITION="right">
<stylenode LOCALIZED_TEXT="styles.topic" COLOR="#18898b" STYLE="fork">
<font NAME="Liberation Sans" SIZE="10" BOLD="true"/>
</stylenode>
<stylenode LOCALIZED_TEXT="styles.subtopic" COLOR="#cc3300" STYLE="fork">
<font NAME="Liberation Sans" SIZE="10" BOLD="true"/>
</stylenode>
<stylenode LOCALIZED_TEXT="styles.subsubtopic" COLOR="#669900">
<font NAME="Liberation Sans" SIZE="10" BOLD="true"/>
</stylenode>
<stylenode LOCALIZED_TEXT="styles.important">
<icon BUILTIN="yes"/>
</stylenode>
</stylenode>
<stylenode LOCALIZED_TEXT="styles.AutomaticLayout" POSITION="right">
<stylenode LOCALIZED_TEXT="AutomaticLayout.level.root" COLOR="#000000">
<font SIZE="18"/>
</stylenode>
<stylenode LOCALIZED_TEXT="AutomaticLayout.level,1" COLOR="#0033ff">
<font SIZE="16"/>
</stylenode>
<stylenode LOCALIZED_TEXT="AutomaticLayout.level,2" COLOR="#00b439">
<font SIZE="14"/>
</stylenode>
<stylenode LOCALIZED_TEXT="AutomaticLayout.level,3" COLOR="#990000">
<font SIZE="12"/>
</stylenode>
<stylenode LOCALIZED_TEXT="AutomaticLayout.level,4" COLOR="#111111">
<font SIZE="10"/>
</stylenode>
</stylenode>
</stylenode>
</map_styles>
</hook>
<hook NAME="AutomaticEdgeColor" COUNTER="3"/>
<node TEXT="UInput" POSITION="right" ID="ID_51575948" CREATED="1534417879505" MODIFIED="1534417886217">
<edge COLOR="#ff0000"/>
<node TEXT="Context" ID="ID_1235527177" CREATED="1534418415327" MODIFIED="1534418417287">
<node ID="ID_443227012" CREATED="1534418428302" MODIFIED="1534419518908"><richcontent TYPE="NODE">

<html>
  <head>
    
  </head>
  <body>
    <p>
      Joystick
    </p>
    <ol>
      <li>
        set event bits
      </li>
      <li>
        set key and axes bits
      </li>
      <li>
        fill name, vendor, bustype, axis border information
      </li>
      <li>
        create virtual device with this information
      </li>
    </ol>
  </body>
</html>

</richcontent>
</node>
<node ID="ID_503618575" CREATED="1534418441454" MODIFIED="1534419693444"><richcontent TYPE="NODE">

<html>
  <head>
    
  </head>
  <body>
    <p>
      Mouse
    </p>
    <ol>
      <li>
        set event bits
      </li>
      <li>
        set button and axis bits
      </li>
      <li>
        fill name, vendor, bustype information
      </li>
      <li>
        create mouse file with this information
      </li>
    </ol>
  </body>
</html>

</richcontent>
</node>
</node>
<node TEXT="Device" ID="ID_897386807" CREATED="1534418419784" MODIFIED="1534418421182">
<node ID="ID_584883596" CREATED="1534419131317" MODIFIED="1534419481194"><richcontent TYPE="NODE">

<html>
  <head>
    
  </head>
  <body>
    <p>
      Joystick
    </p>
    <ol>
      <li>
        for all events fill time, type, code, value information
      </li>
      <li>
        write all events
      </li>
      <li>
        fill information for synchronize event
      </li>
      <li>
        write synchronize event
      </li>
    </ol>
  </body>
</html>

</richcontent>
</node>
</node>
</node>
<node TEXT="VJoy" POSITION="left" ID="ID_1560063078" CREATED="1534417888346" MODIFIED="1534417894224">
<edge COLOR="#0000ff"/>
<node ID="ID_1009159700" CREATED="1534425975677" MODIFIED="1534426565325"><richcontent TYPE="NODE">

<html>
  <head>
    
  </head>
  <body>
    <p>
      Context
    </p>
    <ol>
      <li>
        check enabled
      </li>
      <li>
        get device status
      </li>
      <li>
        acquire device
      </li>
      <li>
        gather device properties
      </li>
    </ol>
  </body>
</html>

</richcontent>
</node>
<node ID="ID_1157926379" CREATED="1534425979417" MODIFIED="1534426634539"><richcontent TYPE="NODE">

<html>
  <head>
    
  </head>
  <body>
    <p>
      Device
    </p>
    <ol>
      <li>
        set joystick device, position properties
      </li>
      <li>
        update joystick
      </li>
    </ol>
  </body>
</html>

</richcontent>
</node>
</node>
<node TEXT="Windows API" POSITION="left" ID="ID_346796925" CREATED="1534417895622" MODIFIED="1534417911756">
<edge COLOR="#00ff00"/>
<node TEXT="SendInput" ID="ID_520998713" CREATED="1534417945234" MODIFIED="1534425935327"/>
</node>
</node>
</map>
