﻿using System;
using LiteDB;

namespace RetroLite.DB.Entity
{
    public class Game
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string System { get; set; }
    }
}