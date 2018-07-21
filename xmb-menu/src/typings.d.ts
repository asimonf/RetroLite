/* SystemJS module definition */
declare var module: NodeModule;
declare module 'regl';
declare module '*.glsl' {
  var _: string;
  export default  _;
}
interface NodeModule {
  id: string;
}
