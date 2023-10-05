var boardSize = 3;
var currentPlayer = '';
document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('#form-setup').addEventListener('submit', function (event) {
        event.preventDefault();
        boardSize = document.getElementById('text-size').value;
        currentPlayer = getSelectedValue();
        generateBoard();
    });
});

function generateBoard() {
    let tBoardBody = document.querySelector('#table-board-body');
    let playerOptions = document.getElementsByName('player');
    tBoardBody.innerHTML = '';
    playerOptions[0].disabled = true;
    playerOptions[1].disabled = true;

    for (let i = 0; i < boardSize; i++) {
        let newRow = document.createElement('tr');
        tBoardBody.appendChild(newRow);
        for (let j = 0; j < boardSize; j++) {
            let inputCol = document.createElement('td');
            let button = document.createElement('button');
            let span = document.createElement('span');
            button.id = 'btn-' + i + '-' + j;
            span.id = 'spn-' + i + '-' + j;
            button.onclick = (function (i, j) {
                return function () {
                    handleInput(i, j);
                }
            })(i, j);
            button.innerText = '-';
            inputCol.appendChild(span);
            inputCol.appendChild(button);
            newRow.appendChild(inputCol);
        }
    }
}

function getSelectedValue() {
    let selectedValue = null;
    let playerOptions = document.getElementsByName('player');

    for (var i = 0; i < playerOptions.length; i++) {

        if (playerOptions[i].checked) {
            selectedValue = playerOptions[i].value;
            break;
        }
    }
    return selectedValue;
}

function switchPlayer() {
    currentPlayer = (currentPlayer === 'X') ? 'O' : 'X';
}

function switchMyPlayer() {
    let playerOptions = document.getElementsByName('player');

    playerOptions[0].checked = !playerOptions[0].checked;
    playerOptions[1].checked = !playerOptions[1].checked;
}

function handleInput(row, column) {
    if (!verifyWinner()) {
        let button = document.getElementById('btn-' + row + '-' + column);
        let span = document.getElementById('spn-' + row + '-' + column);
        span.innerHTML = currentPlayer;
        button.style.display = 'none';
        switchPlayer();
    } else {
        switchPlayer();
        showMsg(`Player ${currentPlayer} wins!`);
        switchMyPlayer();
        switchPlayer();
        generateBoard();

    }
    if (isBoardFull()) {
        switchMyPlayer();
        switchPlayer();

        generateBoard();

        showMsg('It is a draw!');
    }


}

function verifyWinner() {
    let hasAWinner = false;
    let isEntireRowSame = false;
    let isEntireColSame = false;
    let isDiagonalSame = false;

    for (let row = 0; row < boardSize; row++) {
        let span = document.getElementById('spn-' + row + '-' + 0);
        if (span.innerHTML != '') {
            isEntireRowSame = true;
            for (let column = 1; column < boardSize; column++) {
                let otherColCell = document.getElementById('spn-' + row + '-' + column);
                if (span.innerHTML != otherColCell.innerHTML) {
                    isEntireRowSame = false;
                    break;
                }
            }
            if (isEntireRowSame)
                break;
        }
        if (!isEntireRowSame) {

            for (let column = 0; column < boardSize; column++) {

                let span = document.getElementById('spn-' + 0 + '-' + column);
                if (span.innerHTML != '') {
                    isEntireColSame = true;
                    for (let row = 1; row < boardSize; row++) {
                        let otherRowCell = document.getElementById('spn-' + row + '-' + column);
                        if (span.innerHTML != otherRowCell.innerHTML) {
                            isEntireColSame = false;
                            break;
                        }
                    }
                }
                if (isEntireColSame)
                    break;
            }
            if (!isEntireColSame) {
                let span = document.getElementById('spn-' + 0 + '-' + 0);
                if (span.innerHTML != '') {
                    isDiagonalSame = true;
                    for (let nextIndex = 1; nextIndex < boardSize; nextIndex++) {
                        let otherDiagonalCell = document.getElementById('spn-' + nextIndex + '-' + nextIndex);
                        if (span.innerHTML != otherDiagonalCell.innerHTML) {
                            isDiagonalSame = false;
                            break;
                        }
                    }
                }

                if (!isDiagonalSame) {
                    let col = boardSize - 1;
                    //for (let index = 0; index< boardSize; index++) {
                    let span = document.getElementById('spn-' + 0 + '-' + col);
                    if (span.innerHTML != '') {
                        isDiagonalSame = true;
                        for (let nextIndex = 1; nextIndex < boardSize; nextIndex++) {
                            col--;
                            let otherDiagonalCell = document.getElementById('spn-' + nextIndex + '-' + col);
                            if (span.innerHTML != otherDiagonalCell.innerHTML) {
                                isDiagonalSame = false;
                                break;
                            }
                        }

                        //}

                    }

                }
            }
        }
        hasAWinner = isEntireRowSame || isEntireColSame || isDiagonalSame;
        return hasAWinner;
    }
}

function isBoardFull() {
    for (let row = 0; row < boardSize; row++) {
        for (let column = 0; column < boardSize; column++) {
            let span = document.getElementById('spn-' + row + '-' + column);

            if (span.innerHTML === '') {
                return false;
            }
        }
    }
    return true;
}

function showMsg(msg) {

    let msgDiv = document.querySelector('#msg');

    msgDiv.classList.add('alert-success');
    msgDiv.classList.remove('fade-out');
    setTimeout(() => {
        msgDiv.classList.remove('alert-success');
        document.querySelector('#msg').innerHTML = '';
        msgDiv.classList.add('fade-out');
        msgDiv.classList.remove('fade-in');
    }, 5000);
    document.querySelector('#msg').innerHTML = msg;
    msgDiv.classList.toggle('fade-in');

}