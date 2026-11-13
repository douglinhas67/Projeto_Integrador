CREATE DATABASE IF NOT EXISTS controle_treinos;
USE controle_treinos;

CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT NOT NULL
) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS treinos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    modalidade VARCHAR(50) NOT NULL,
    data DATE NOT NULL,
    duracao_minutos INT NOT NULL,

    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE = InnoDB;