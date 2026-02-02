CREATE TABLE IF NOT EXISTS tarifacao (
    idtarifacao TEXT PRIMARY KEY,
    idcontacorrente TEXT NOT NULL,
    valortarifado REAL NOT NULL,
    datahoratarifacao TEXT NOT NULL,
    requestid TEXT NOT NULL UNIQUE
);

CREATE INDEX IF NOT EXISTS idx_tarifacao_requestid ON tarifacao(requestid);
CREATE INDEX IF NOT EXISTS idx_tarifacao_conta ON tarifacao(idcontacorrente);
