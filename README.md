# 概要

Unityでプロシージャルな内装生成システムを作成するために開発している個人プロジェクトです。

もともとはUnreal Engine/C++で開発していたプロシージャル生成アルゴリズムを、よりスペックの低い環境でも動作させられるよう、現在はUnity向けに開発しています。

このアルゴリズムでは、UnityのSpline Componentを使って内装全体の形状を指定し、Splineの内側を生成した部屋を配置するためのスペースとして使用します。

## プロジェクトの目的

異なるジャンルのゲームで利用できる、汎用性の高いプロシージャル内装生成システムを作ることを目的としています。また、このシステムの設計・実装を通して、自身のプログラミングスキルを高めることも目的としています。

アルゴリズムは、以下の点を意識して設計しています。

1. **異なるゲームプロジェクトでの再利用性**
2. **部屋の広さや利用可能なスペースなど、条件に合った部屋を開発者が選びやすい仕組み**
3. **単純なランダム生成ではなく、人の手で作られたような自然な部屋構成**
4. **隣接するすべての部屋を直接つなげず、迷路のようなレベル構成を作りながら、すべての部屋へ到達できる状態を維持**
5. **壁・床・天井などの構造データを最適化し、各サンプルごとにメッシュを生成するのではなく、複数のサンプルをまとめてメッシュを生成することで、生成するオブジェクト数を抑える**
6. **静的なオブジェクトを配置して部屋が空間的に感じられないようにしつつ、重要な通路や移動経路を家具などで塞がないようにする**
7. **将来的に異なるゲームプロジェクトに合わせて機能の追加やカスタマイズができる拡張性**
8. **マルチプレイへの対応を想定した設計**. 生成したレベルをネットワーク経由で同期する際に必要となる情報をできるだけ少なくし、ネットワーク上で送受信するデータ量を抑えられるよう設計

# レベルプロシージャル生成アルゴリズム概要（現在の実装状況）

## グリッド生成
UnityのSpline Componentをレベル全体の形状として使用し、その範囲をグリッドに変換して、生成可能な範囲を決定します。

## 部屋の生成
使用可能なサンプルポイントからランダムに起点を選び、計算した部屋のサイズに合わせて外側へ拡張しながら部屋を生成します。また、各部屋が占有する範囲を計算し、床や天井のメッシュ生成を効率化します。

## 壁と隣接関係の生成
部屋同士や部屋と外側との境界から壁のサンプルを生成し、そこから隣接する部屋の関係を決定します。

## ドアの配置と接続性の確保
隣接する部屋同士の接続をランダムに維持または切断することで、部屋の構成に変化をつけます。その際、すべての部屋が互いに到達可能な状態を維持します。接続を維持する箇所では、壁のサンプルからランダムに1つを選び、ドアとして配置します。

## 壁の最適化
壁の向きや直線性をもとに壁のサンプルを統合し、生成する壁の数を減らすことで、メッシュ生成を効率化します。



# Overview

This is a personal project developed in Unity to create a procedural interior generation system.

The procedural generation algorithm was originally developed using Unreal Engine and C++. It is currently being developed in Unity to allow the system to run on lower-spec hardware.

The algorithm uses Unity's Spline Component to define the overall shape of the interior. The space within the spline is then used as the area where generated rooms can be placed.

## Project Objectives

The goal of this project is to create a versatile procedural interior generation system that can be used across different genres of games. Developing this system also provides an opportunity to improve my own programming and system design skills.

The algorithm is designed with the following goals in mind:

1. **Reusability across different game projects**
2. **Allowing developers to select rooms based on specific conditions**, such as room dimensions and available space
3. **Creating natural-looking room layouts** rather than relying on purely random placement
4. **Maintaining room connectivity while creating a maze-like layout** by not directly connecting every room to all of its immediate neighbours
5. **Optimizing structural data for walls, floors, and ceilings** by combining multiple samples into larger sections rather than generating a mesh for every individual sample, reducing the number of objects that need to be generated
6. **Furnishing rooms with static objects** to make spaces feel less empty while ensuring that furniture does not block important paths or traversal routes
7. **Scalability and extensibility**, allowing the system to be expanded or customized for future game projects
8. **Multiplayer and network-friendly design**, minimize the amount of information that needs to be replicated across the network when synchronizing the generated level, reducing the amount of data that needs to be transmitted.

# Level Procedural Generation Algorithm (Current State)

## Grid Generation

A Unity Spline Component is used to define the overall shape of the level. The area within the spline is converted into a grid to determine the space available for generation.

## Room Generation

A random available sample point is selected as the starting point for each room. The room is then expanded outward based on its calculated desired size.

The space occupied by each room is also calculated and stored to optimize the generation of floor and ceiling meshes.

## Wall and Neighbour Generation

Wall samples are generated from the boundaries between rooms and between rooms and the outside area. These boundaries are then used to determine the neighbouring relationships between rooms.

## Door Placement and Connectivity

Connections between neighbouring rooms are randomly selected to either remain connected or be broken, creating variation in the generated layout.

While doing so, the algorithm ensures that all rooms remain reachable from one another. For connections that remain open, a wall sample is randomly selected and converted into a door.

## Wall Optimization

Wall samples are merged based on their direction and linearity, reducing the number of wall segments that need to be generated and improving the efficiency of the resulting mesh generation.
