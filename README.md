# Terrain with Carved Paths Generation Tool

[![Status](https://img.shields.io/badge/status-in%20development-yellow)](#)
[![License](https://img.shields.io/badge/license-Custom--Non--Commercial-lightgrey)](#)

> ⚠️ This project is licensed for **non-commercial use only**. See LICENSE for details.

<br>

### 🇬🇧 English 

This project includes a **parametrized terrain generation tool** capable of creating maps with a network of paths carved directly onto the terrain.  
Both the terrain and the paths feature multiple configurable options to achieve highly varied results.

### ✨ Main Features

- **Parametrized terrain creation tool** with many configuration parameters allowing for very diverse terrains.
- Terrain generation system based on the "Landmass Generation" approach proposed by Sebastian Lague.
- Configurable color-based texturing according to terrain orography.
- Terrain divided into generation chunks allowing for infinite terrains.
- Segregation of points of interest on the map with natural distribution in allowed areas using the Poisson-Disk Sampling algorithm.
- Connection between points of interest using Delaunay triangulation (Habrador Computational Geometry Library).
- Drawing of curves between points of interest with multiple levels of recursion to generate more intricate and jagged curves.
- Path carving system that adapts the terrain geometry to the curve layout with configurable curve profiles.

### 🎯 Project Objectives

- To carry out a technically demanding personal project involving advanced programming knowledge.
- To achieve a powerful and versatile terrain generation system with paths for personal use.
- To create a well-documented tool with good usability.
- To develop a scalable and maintainable code architecture.

### 🚀 Future Development

- Make the path carving system work with chunk-based terrain generation (starting from tag v0.1).
- Improve documentation.
- Improve documentation in many methods to allow freer use.
- Improve the editor tool interface to simplify and explain parameters used during generation.

---

<br>

### 🇪🇸 Spanish 

Este proyecto incluye una herramienta de **generación de terrenos parametrizada**, capaz de crear mapas con una red de caminos esculpidos directamente sobre el terreno. Tanto el terreno como los caminos cuentan con múltiples opciones de configuración para obtener resultados muy variados.

### ✨ Características principales

- Herramienta de creación de terrenos con multitud de parámetros de configuración que permite crear terrenos muy variados.
- Sistema de generación de terreno basado en el sistema de "Landmass Generation" propuesto por Sebastian Lague.
- Generación de textura configurable por colores en función de la orografía del terreno.
- Terreno separado en chunks de generación que permite crear terrenos infinitos. 
- Segregación de puntos de interés sobre el mapa con una distribución natural por zonas permitidas del mapeado mediante el algoritmo de Poisson-Disk Sampling.
- Unión entre los puntos de interés a partir del algoritmo de triangulación de Delaunay (Habrador Computational Geometry Library).
- Dibujado de curvas entre puntos de interés con varios niveles de recursividad para generar curvas más enrevesadas y temblorosas.
- Sistema de carving de curvas que adaptan el terreno al trazado con geometría y un perfil de curva configurable.

### 🎯 Objetivos del proyecto

- Realizar un proyecto personal técnicamente exigente con conocimientos de programación más avanzados.
- Conseguir un sistema de generación de terreno con caminos potente y versátil para uso personal.
- Crear una herramienta bien documentada con buena usabilidad.
- Conseguir una arquitectura de código escalable y mantenible.

### 🚀 Futuro desarrollo

- Hacer que el sistema de path carving funcione con la generación por chunks del terreno (a partir de tag v0.1).
- Mejorar la documentación.
- Mejorar documentación en buena parte de los métodos para permitir utilizarlos con mayor libertad.
- Mejorar la interfaz de la herramienta de editor para simplificar y explicar los parámetros utilizados durante la generación.
