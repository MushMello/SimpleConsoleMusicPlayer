# SimpleConsoleMusicPlayer

Commands (not case sensitive):

play [PathToMusicFile]:<br/>
Plays the selected file. Can be used without path if player is only paused

pause:<br/>
Pauses the current song

stop:<br/>
Stops the player from playing. Doesn't unload the played song, but it will start from the beginning if "play" is used afterwards

where:<br/>
Shows a timeline to visualize duration and current position

jumpto [seconds | timeStamp (mm:ss)]:<br/>
Jumps to the given seconds/to the given time stamp

skiptime [seconds]:<br/>
skips [seconds] of the song

help:<br/>
Shows the commands
