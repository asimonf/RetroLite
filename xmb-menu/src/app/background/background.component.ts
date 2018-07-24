import {Component, OnInit, ViewChild, ElementRef, AfterViewInit} from '@angular/core';
import * as createREGL from 'regl';

const BackgroundVertex = `
precision lowp float;

attribute vec2 position;

uniform vec2 resolution;

varying highp vec2 pos;
varying float gradient;

void main() {
  pos = ( position + 1.0 ) / 2.0 * resolution;
  gradient = 1.0 - position.y * 0.625;
  gl_Position = vec4( position, 0.0, 1.0 );
}
`;

const BackgroundFragment = `
precision lowp float;

uniform vec3 color;
uniform vec2 resolution;
uniform sampler2D bayerTexture;

varying highp vec2 pos;
varying float gradient;

const float colorDepth = 255.0;
vec3 dither( vec2 position, vec3 color ) {
  float threshold = texture2D( bayerTexture, position / 8.0 ).a;
  vec3 diff = 1.0 - mod( color * colorDepth, 1.0 );
  return color + diff * vec3(
      float( diff.r < threshold ),
      float( diff.g < threshold ),
      float( diff.b < threshold )
    ) / colorDepth;
}
void main() {
  gl_FragColor = vec4( dither( pos, gradient * color ), 1.0 );
}
`;

const FlowVertex = `
precision lowp float;

attribute vec2 position;

uniform float time;
uniform float ratio;
uniform float step;
uniform float opacity;

varying float alpha;

float iqhash( float n ) {
  return fract( sin( n ) * 43758.5453 );
}

float noise( vec3 x ) {
  vec3 f = fract( x );
  f = f * f * ( 3.0 - 2.0 * f );
  float n = dot( floor( x ), vec3( 1.0, 57.0, 113.0 ) );
  return mix(
        mix( mix( iqhash( n +   0.0 ), iqhash( n +   1.0 ), f.x ),
           mix( iqhash( n +  57.0 ), iqhash( n +  58.0 ), f.x ),
           f.y ),
        mix(
           mix( iqhash( n + 113.0 ), iqhash( n + 114.0 ), f.x ),
           mix( iqhash( n + 170.0 ), iqhash( n + 171.0 ), f.x ),
           f.y ),
        f.z );
}

vec3 getVertex( float x, float y ) {
  vec3 vertex = vec3( x, cos( y * 4.0 ) * cos( y + time / 5.0 + x ) / 8.0, y );

  float c = noise( vertex * vec3( 7.0 / 4.0, 7.0, 7.0 ) ) / 15.0;
  vertex.y += c + cos( x * 2.0 - time ) * ratio / 2.0 - 0.3;
  vertex.z += c;

  return vertex;
}

void main() {
  gl_Position = vec4( getVertex( position.x, position.y ), 1.0 );

  vec3 dfdx = getVertex( position.x + step, position.y ) - gl_Position.xyz;
  vec3 dfdy = getVertex( position.x, position.y + step ) - gl_Position.xyz;
  alpha = 1.0 - abs( normalize( cross( dfdx, dfdy ) ).z );
  alpha = ( 1.0 - cos( alpha * alpha ) ) * opacity;
}`;

const FlowFragment = `
precision lowp float;

varying float alpha;

void main() {
  gl_FragColor = vec4( alpha, alpha, alpha, 1.0 );
}`;

const ParticleVertex = `
precision lowp float;

attribute vec3 seed;
uniform float time;
uniform float ratio;
uniform float opacity;

varying float alpha;

float getWave( float x, float y ) {
  return cos( y * 4.0 ) * cos( x + y + time / 5.0 ) / 8.0 + cos( x * 2.0 - time ) * ratio / 2.0 - 0.28;
}

void main() {
  gl_PointSize = seed.z;

  float x = fract( time * ( seed.x - 0.5 ) / 15.0 + seed.y * 50.0 ) * 2.0 - 1.0;
  float y = sin( sign( seed.y ) * time * ( seed.y + 1.5 ) / 4.0 + seed.x * 100.0 );
  y /= ( 6.0 - seed.x * 4.0 * seed.y ) / ratio;

  float opacityVariance = mix(
    sin( time * ( seed.x + 0.5 ) * 12.0 + seed.y * 10.0 ),
    sin( time * ( seed.y + 1.5 ) * 6.0 + seed.x * 4.0 ),
    y * 0.5 + 0.5 ) * seed.x + seed.y;
  alpha = opacity * opacityVariance * opacityVariance;

  y += getWave( x, seed.y );

  gl_Position = vec4( x, y, 0.0, 1.0 );
}`;

const ParticleFragment = `
precision lowp float;

varying float alpha;

void main() {
  vec2 cxy = gl_PointCoord * 2.0 - 1.0;
  float radius = dot( cxy, cxy );
  gl_FragColor = vec4( vec3( alpha * max( 0.0, 1.0 - radius * radius ) ), 1.0 );
}`;

@Component({
  selector: 'app-background',
  templateUrl: './background.component.html',
  styleUrls: ['./background.component.css']
})
export class BackgroundComponent implements AfterViewInit {
  private RESOLUTION = 50;
  private NUM_PARTICLES = 400;
  private NUM_VERTICES = this.RESOLUTION * this.RESOLUTION + (this.RESOLUTION + 2) * (this.RESOLUTION - 2) + 2;
  private PARTICLE_SIZE = 8;

  private lastTime;
  private tick;

  constructor(private elRef: ElementRef) {
  }

  ngAfterViewInit(): void {
    this.reInit();
  }

  reInit(): void {
    this.lastTime = 0;

    if (this.tick) {
      this.tick.cancel();
      this.tick = null;
    }

    const regl = createREGL({
      container: this.elRef.nativeElement,
      attributes: {
        antialias: false
      },
      optionalExtensions: ['EXT_disjoint_timer_query'],
      profile: false
    });

    const drawBackground = regl({
      vert: BackgroundVertex,
      frag: BackgroundFragment,
      primitive: 'triangle strip',
      count: 4,
      attributes: {
        position: [
          +1, -1,
          -1, -1,
          +1, +1,
          -1, +1
        ]
      },
      uniforms: {
        color: regl.prop('color'),
        resolution: (context, props) => [context.viewportWidth, context.viewportHeight],
        bayerTexture: regl.texture({
          data: Uint8Array.of(
            0, 128, 32, 160, 8, 136, 40, 168,
            192, 64, 224, 96, 200, 72, 232, 104,
            48, 176, 16, 144, 56, 184, 24, 152,
            240, 112, 208, 80, 248, 120, 216, 88,
            12, 140, 44, 172, 4, 132, 36, 164,
            204, 76, 236, 108, 196, 68, 228, 100,
            60, 188, 28, 156, 52, 180, 20, 148,
            252, 124, 220, 92, 244, 116, 212, 84
          ),
          format: 'alpha',
          shape: [8, 8],
          wrap: ['repeat', 'repeat']
        })
      },
      dither: false,
      depth: {enable: false}
    });

    const drawFlow = regl({
      vert: FlowVertex,
      frag: FlowFragment,
      primitive: 'triangle strip',
      count: this.NUM_VERTICES,
      attributes: {
        position: this.makeFlowVertices()
      },
      uniforms: {
        time: regl.prop('time'),
        opacity: regl.prop('opacity'),
        ratio: regl.prop('ratio'),
        step: 2 / this.RESOLUTION
      },
      blend: {
        enable: true,
        func: {src: 1, dst: 1}
      },
      dither: false
    });

    const drawParticles = regl({
      vert: ParticleVertex,
      frag: ParticleFragment,
      primitive: 'points',
      count: this.NUM_PARTICLES,
      attributes: {
        seed: this.makeParticleSeeds()
      },
      uniforms: {
        time: regl.prop('time'),
        ratio: regl.prop('ratio'),
        opacity: regl.prop('particleOpacity')
      },
      blend: {
        enable: true,
        func: {src: 1, dst: 1}
      },
      dither: false,
      depth: {enable: false}
    });

    const drawParams = {
      time: 0,
      color: [0, 0, 0],
      backgroundColor: [37, 89, 179],
      flowSpeed: 0.25,
      opacity: 0.75,
      brightness: (1 - Math.cos(0.5 * 2 * Math.PI)) / 1.75,
      particleOpacity: 0.75,
      ratio: 0
    };

    this.tick = regl.frame((context) => {
      drawParams.backgroundColor.forEach((channel, i) => drawParams.color[i] = channel * drawParams.brightness / 255);
      drawParams.ratio = Math.max(1.0, Math.min(context.viewportWidth / context.viewportHeight, 2.0)) * 0.375;
      drawParams.time = drawParams.time + (context.time - this.lastTime) * drawParams.flowSpeed;
      this.lastTime = context.time;
      drawBackground(drawParams);
      drawFlow(drawParams);
      drawParticles(drawParams);
    });
  }

  makeFlowVertices() {
    const vertices = new Float32Array(this.NUM_VERTICES * 3);
    const yPos = new Float32Array(this.RESOLUTION);
    for (let y = 0; y < this.RESOLUTION; y++) {
      yPos[y] = y / (this.RESOLUTION - 1) * 2 - 1;
    }
    let xPos1 = -1;
    let numVertices = 0;
    for (let x = 1; x < this.RESOLUTION; x++) {
      const xPos2 = x / (this.RESOLUTION - 1) * 2 - 1;
      vertices[numVertices++] = xPos2;
      vertices[numVertices++] = -1;
      for (let y = 0; y < this.RESOLUTION; y++) {
        vertices[numVertices++] = xPos2;
        vertices[numVertices++] = yPos[y];
        vertices[numVertices++] = xPos1;
        vertices[numVertices++] = yPos[y];
      }
      vertices[numVertices++] = xPos1;
      vertices[numVertices++] = 1;
      xPos1 = xPos2;
    }
    return vertices;
  }

  makeParticleSeeds() {
    const seeds = new Float32Array(this.NUM_PARTICLES * 3);
    let numSeeds = 0;
    for (let i = 0; i < this.NUM_PARTICLES; i++) {
      seeds[numSeeds++] = Math.random();
      seeds[numSeeds++] = Math.random();
      seeds[numSeeds++] = Math.pow(Math.random(), 10) * this.PARTICLE_SIZE + 3;
    }
    return seeds;
  }
}
