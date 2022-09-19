# ProjectDungeon

Make 2d roguelike game from Udemy
(=> https://www.udemy.com/share/105Z9y3@VsfgC929Ht3oh0sByPa7i4mhBZls-CZLtRS7dypJv7w6HurOmdrMH1a4AzF-wXMQ/)

## Room Node Creater
![Create_RoomNodeGraph](https://user-images.githubusercontent.com/47097472/188354664-7079311c-c410-4935-b254-47e72b4c658b.gif)
- 첫 노드 생성시 자동으로 ‘Entrance’(시작 지점)이 생성됨.
- ‘Entrance’에는 ‘Corridor’만 연결 가능.
- 방과 방사이에 ‘Corridor’가 무조건 있어야함.
- ‘Boss Room’에는 항상 하나의 ‘Corridor’ 만 연결할 수 있음.

## Dungeon Building Algorithm
1. DungeonLevel에서 무작위 RoomNodeGraph를 선택한다.
2. 그래프로부터 'Entrance Node'를 "OpenRoomNodeQueue"에 추가한다.
3. 'Queue'에 'Room Node' 가 더 있을 경우.
4. 'Queue'로 부터 다음 'Room Node'를 받는다.
5. 