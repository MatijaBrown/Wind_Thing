﻿using NIMBY.States;
using NIMBY.Tiles;
using NIMBY.Utils;
using Silk.NET.OpenGL;
using System.Numerics;

namespace NIMBY.Graphics.Renderers
{
    public class TileRenderer
    {

        private readonly Shader _shader;
        private readonly VAO _vao;
        private readonly GL _gl;

        private readonly Game _game;

        public TileRenderer(GameState _state)
        {
            _game = _state.Manager.Game;
            _gl = _game.Gl;
            _shader = ResourceManager.LoadShader("tileVertexShader", "tileFragmentShader");
            _vao = ResourceManager.CreateVao();
            InitRenderData();
        }

        private void InitRenderData()
        {
            _vao.StoreData(Tile.VERTICES, 0, 2, GLEnum.Float);
            uint[] tcoords =
            {
                0, 1,
                0, 0,
                1, 0,
                0, 1,
                1, 0,
                1, 1
            };
            _vao.StoreData(tcoords, 1, 2, GLEnum.UnsignedInt);
        }

        public void Render(Tile tile, Camera camera)
        {
            _shader.Start();

            Matrix4x4 transformation = Maths.CreateTransformationMatrix(tile.DrawX, tile.DrawY, camera.Scale);
            _shader.LoadMatrix("transformation", transformation);
            Matrix4x4 viewMatrix = Maths.CreateViewMatrix(camera);
            _shader.LoadMatrix("viewMatrix", viewMatrix);
            float aspectRatio = (float)_game.Witdh / _game.Height;
            Matrix4x4 projection = Maths.CreateProjectionMatrix(_game.Height * aspectRatio, _game.Height); ;
            _shader.LoadMatrix("projection", projection);

            _vao.Bind();
            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);

            Texture texture = TileTexture.Get(tile.TileType);
            texture.Bind();
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            texture.Unbind();

            if (tile.Turbined)
            {
                TileTexture.Turbine.Bind();
                _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
                TileTexture.Turbine.Unbind();
            }

            _gl.DisableVertexAttribArray(0);
            _gl.DisableVertexAttribArray(1);
            _vao.Unbind();

            _shader.Stop();
        }

    }
}
