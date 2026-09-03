(function () {
  "use strict";

  var TURMAS_ENDPOINT = "/api/turmas";
  var MENSAGEM_ERRO_REDE = "Não foi possível conectar à API. Tente novamente.";

  var btnListar = document.getElementById("btn-listar");
  var btnLabel = btnListar.querySelector(".btn-label");
  var btnSpinner = btnListar.querySelector(".spinner");

  var modalDados = document.getElementById("modal-dados");
  var modalErro = document.getElementById("modal-erro");
  var tbody = document.getElementById("turmas-tbody");
  var mensagemErroEl = document.getElementById("modal-erro-mensagem");

  function fecharModais() {
    modalDados.hidden = true;
    modalErro.hidden = true;
    document.removeEventListener("keydown", onKeydown);
  }

  function abrirModal(modal) {
    fecharModais();
    modal.hidden = false;
    document.addEventListener("keydown", onKeydown);
  }

  function onKeydown(event) {
    if (event.key === "Escape") {
      fecharModais();
    }
  }

  document.querySelectorAll("[data-close]").forEach(function (botao) {
    botao.addEventListener("click", fecharModais);
  });

  [modalDados, modalErro].forEach(function (overlay) {
    overlay.addEventListener("click", function (event) {
      if (event.target === overlay) {
        fecharModais();
      }
    });
  });

  function definirCarregando(carregando) {
    btnListar.disabled = carregando;
    btnSpinner.hidden = !carregando;
    btnLabel.textContent = carregando ? "Carregando..." : "Listar turmas";
  }

  function celula(label, valor) {
    var td = document.createElement("td");
    td.setAttribute("data-label", label);
    td.textContent = valor;
    return td;
  }

  function montarLinhaVazia() {
    var tr = document.createElement("tr");
    var td = document.createElement("td");
    td.colSpan = 5;
    td.className = "empty-cell";
    td.textContent = "Nenhum dado encontrado.";
    tr.appendChild(td);
    return tr;
  }

  function montarLinhaTurma(turma) {
    var vagasOcupadas = turma.VagasTotal - turma.VagasDisponiveis;
    var tr = document.createElement("tr");

    tr.appendChild(celula("Id", turma.Id));
    tr.appendChild(celula("Nome", turma.Nome));
    tr.appendChild(celula("Vagas totais", turma.VagasTotal));
    tr.appendChild(celula("Vagas disponíveis", turma.VagasDisponiveis));
    tr.appendChild(celula("Ocupação", vagasOcupadas + " / " + turma.VagasTotal));

    return tr;
  }

  function exibirDados(turmas) {
    tbody.innerHTML = "";

    if (!turmas || turmas.length === 0) {
      tbody.appendChild(montarLinhaVazia());
    } else {
      turmas.forEach(function (turma) {
        tbody.appendChild(montarLinhaTurma(turma));
      });
    }

    abrirModal(modalDados);
  }

  function exibirErro(mensagem) {
    mensagemErroEl.textContent = mensagem || MENSAGEM_ERRO_REDE;
    abrirModal(modalErro);
  }

  function listarTurmas() {
    definirCarregando(true);

    fetch(TURMAS_ENDPOINT, { headers: { Accept: "application/json" } })
      .then(function (response) {
        return response
          .json()
          .catch(function () {
            return null;
          })
          .then(function (corpo) {
            return { ok: response.ok, corpo: corpo };
          });
      })
      .then(function (resultado) {
        var corpo = resultado.corpo;

        if (!resultado.ok || !corpo || corpo.ContemErros) {
          exibirErro(corpo && corpo.Mensagem);
          return;
        }

        exibirDados(corpo.Dados);
      })
      .catch(function () {
        exibirErro(null);
      })
      .finally(function () {
        definirCarregando(false);
      });
  }

  btnListar.addEventListener("click", listarTurmas);
})();
