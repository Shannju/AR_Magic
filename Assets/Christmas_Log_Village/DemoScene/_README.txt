To get the demo scenes working, you need to go to File > build setting and assing the level as :

0 - GameStart
1 - Christmas_Log_Village_Night OR Christmas_Log_Village_Day
2 - Christmas_Log_Village_Interior

To make the script work, your Player prefab need to have the "Player" tag and need to be child of the GameManager in the GameStart scene. (See GameManager.cs) for more info.

If you don't need the door transition logic, you can delete all the "Trigger_..." and "_FadeCam" in scenses.