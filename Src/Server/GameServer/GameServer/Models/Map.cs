using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;

using Common;
using Common.Data;

using Network;
using GameServer.Managers;
using GameServer.Entities;
using GameServer.Services;


namespace GameServer.Models
{
    class Map
    {
        internal class MapCharacter
        {
            public NetConnection<NetSession> connection;
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }

        public int ID
        {
            get { return this.Define.ID; }
        }

        internal MapDefine Define;

        Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();

        internal Map(MapDefine define)
        {
            this.Define = define;
        }

        internal void Update()
        {

        }


        /// <summary>
        /// 角色进入地图
        /// </summary>
        /// <param name="conn">网络会话</param>
        /// <param name="character">网络信息</param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}",this.Define.ID,character.Id);

            //标记为我当前已经进入哪张地图
            character.Info.mapId = this.ID;

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            //角色进入地图响应
            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character.Info);

            //进入一张地图 看看地图中有没有其他角色 把进入游戏的信息发送给其他角色
            //进入游戏先告诉别人 在构建信息
            foreach (var kv in this.MapCharacters)
            {
                message.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);
                this.SendCharacterEnterMap(kv.Value.connection, character.Info);
            }
            //自己的信息 在这里构建
            this.MapCharacters[character.Id] = new MapCharacter(conn,character);
            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data,0,data.Length);
        }
        /// <summary>
        /// 角色离开
        /// </summary>
        /// <param name="cha"></param>
        internal void CharacterLeave(Character cha)
        {
            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}",this.Define.ID,cha.Id);
            foreach (var kv in this.MapCharacters)
            {
                this.SendCharacterLeaveMap(kv.Value.connection,cha);
            }
            this.MapCharacters.Remove(cha.Id);

        }


        //角色进入地图广播逻辑
        void SendCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character);

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }

        private void SendCharacterLeaveMap(NetConnection<NetSession> coon, Character character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            message.Response.mapCharacterLeave.characterId = character.Id;

            byte[] data = PackageHandler.PackMessage(message);
            coon.SendData(data, 0, data.Length);
        }

        internal void UpdateEntity(NEntitySync entity)
        {
            foreach (var kv in MapCharacters)
            {
                //遍历每个角色 如果他是我自己，就把位置方向速度更新到服务器
                if (kv.Value.character.entityId == entity.Id)
                {
                    kv.Value.character.Position = entity.Entity.Position;
                    kv.Value.character.Direction = entity.Entity.Direction;
                    kv.Value.character.Speed = entity.Entity.Speed;
                }
                else
                {
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entity);
                }
            }
        }
    
    }
}
