package io.github.naji.pokemongo.coin.bag.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import io.github.naji.pokemongo.coin.bag.AppState

private fun stateLabel(state: AppState): String = when (state) {
    AppState.Idle -> "Pronto"
    AppState.Scanning -> "A procurar…"
    AppState.Connecting -> "A ligar…"
    AppState.ReadingDevice -> "A ler…"
    AppState.Pairing -> "A emparelhar…"
    AppState.Exchanging -> "A receber…"
    is AppState.SuccessPair -> "Emparelhado!"
    is AppState.SuccessExchange -> "Postal OK!"
    is AppState.Failure -> "Erro"
}

private fun stateColor(state: AppState): Color = when (state) {
    is AppState.SuccessPair, is AppState.SuccessExchange -> Color(0xFF48BB78)
    is AppState.Failure -> Color(0xFFF56565)
    AppState.Idle -> Color(0xFF667EEA)
    else -> Color(0xFFECC94B)
}

@Composable
fun WearStatusScreen(
    state: AppState,
    permissionDenied: Boolean,
    isScanning: Boolean,
    onToggleScan: () -> Unit
) {
    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black),
        contentAlignment = Alignment.Center
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(14.dp)
                .verticalScroll(rememberScrollState()),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                text = "PoGo Coin Bag",
                fontSize = 12.sp,
                fontWeight = FontWeight.Bold,
                color = Color.White.copy(alpha = 0.7f)
            )

            Spacer(modifier = Modifier.height(6.dp))

            // Cartão de estado circular ou oval compacto
            Box(
                modifier = Modifier
                    .fillMaxWidth(0.9f)
                    .background(stateColor(state), RoundedCornerShape(18.dp))
                .padding(vertical = 10.dp, horizontal = 8.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text(
                        text = stateLabel(state),
                        color = Color.Black,
                        fontSize = 15.sp,
                        fontWeight = FontWeight.Black,
                        textAlign = TextAlign.Center
                    )
                    
                    val detail = when (state) {
                        is AppState.SuccessPair -> state.playerName
                        is AppState.SuccessExchange -> state.playerName
                        is AppState.Failure -> state.message ?: "Falha"
                        else -> null
                    }
                    if (detail != null) {
                        Text(
                            text = detail,
                            color = Color.Black.copy(alpha = 0.8f),
                            fontSize = 10.sp,
                            maxLines = 1
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.height(10.dp))

            // Botão Principal de Ativação do Relógio
            Button(
                onClick = onToggleScan,
                modifier = Modifier
                    .fillMaxWidth(0.85f)
                    .height(44.dp),
                shape = RoundedCornerShape(22.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = if (isScanning) Color(0xFFE53E3E) else Color(0xFF3182CE)
                )
            ) {
                Text(
                    text = if (isScanning) "Parar" else "Conectar",
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold,
                    color = Color.White
                )
            }
        }
    }
}
