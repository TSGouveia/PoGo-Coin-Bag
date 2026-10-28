package io.github.naji.pokemongo.coin.bag.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import io.github.naji.pokemongo.coin.bag.AppState
import io.github.naji.pokemongo.coin.bag.LogEntry
import io.github.naji.pokemongo.coin.bag.LogEventType
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter

private fun stateLabel(state: AppState): String = when (state) {
    AppState.Idle -> "Parado (Pronto)"
    AppState.Scanning -> "A procurar..."
    AppState.Connecting -> "A conectar..."
    AppState.ReadingDevice -> "A verificar..."
    AppState.Pairing -> "A emparelhar..."
    AppState.Exchanging -> "A receber postal..."
    is AppState.SuccessPair -> "Emparelhado!"
    is AppState.SuccessExchange -> "Postal recebido!"
    is AppState.Failure -> "Falhou"
}

private fun stateColor(state: AppState): Color = when (state) {
    is AppState.SuccessPair, is AppState.SuccessExchange -> Color(0xFF69D391)
    is AppState.Failure -> Color(0xFFFF7C8C)
    AppState.Idle -> Color(0xFF8B79FF)
    else -> Color(0xFFD2A63D)
}

@Composable
fun StatusScreen(
    state: AppState,
    permissionDenied: Boolean,
    log: List<LogEntry> = emptyList(),
    isScanning: Boolean = false,
    onToggleScan: () -> Unit = {}
) {
    Surface(color = Color(0xFF11131A)) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            if (permissionDenied) {
                StatusCard(
                    label = "Permissão necessária",
                    detail = "Conceda permissões de Bluetooth e Localização para continuar",
                    color = Color(0xFF4A4D5C),
                )
            } else {
                val detail = when (state) {
                    is AppState.Failure -> state.message
                    is AppState.SuccessPair -> "Emparelhado com ${state.playerName}"
                    is AppState.SuccessExchange -> "Postal recebido de ${state.playerName}"
                    else -> if (!isScanning) "Pressione o botão para iniciar" else "À espera do Pokémon GO..."
                }
                StatusCard(
                    label = stateLabel(state),
                    detail = detail,
                    color = stateColor(state),
                )
            }

            Spacer(modifier = Modifier.height(20.dp))

            // Botão de Controlo Manual (Semelhante ao Website)
            Button(
                onClick = onToggleScan,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp),
                shape = RoundedCornerShape(16.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = if (isScanning) Color(0xFFE53E3E) else Color(0xFF4F46E5)
                )
            ) {
                Text(
                    text = if (isScanning) "Parar Ligação" else "Conectar Switch (Iniciar)",
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold,
                    color = Color.White
                )
            }

            Spacer(modifier = Modifier.height(24.dp))
            HistoryCard(log)

            Spacer(modifier = Modifier.height(24.dp))
            InstructionsCard()
        }
    }
}

private val timeFormatter: DateTimeFormatter =
    DateTimeFormatter.ofPattern("HH:mm:ss").withZone(ZoneId.systemDefault())

private fun logIcon(type: LogEventType): String = when (type) {
    LogEventType.PAIRED -> "🔗"
    LogEventType.EXCHANGED -> "📮"
    LogEventType.FAILED -> "⚠️"
}

private fun logSummary(entry: LogEntry): String = when (entry.type) {
    LogEventType.PAIRED -> "Emparelhado com ${entry.playerName}"
    LogEventType.EXCHANGED -> "Postal recebido de ${entry.playerName}"
    LogEventType.FAILED -> entry.message ?: "Falhou"
}

@Composable
private fun HistoryCard(log: List<LogEntry>) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .background(Color(0x0AFFFFFF), RoundedCornerShape(24.dp))
            .padding(20.dp),
    ) {
        Text("Histórico", color = Color.White, fontWeight = FontWeight.Bold)
        Spacer(modifier = Modifier.height(12.dp))
        if (log.isEmpty()) {
            Text("Nenhuma atividade recente.", color = Color.White.copy(alpha = 0.6f), fontSize = 13.sp)
        } else {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .heightIn(max = 240.dp)
                    .verticalScroll(rememberScrollState()),
            ) {
                val entries = log.asReversed()
                entries.forEachIndexed { index, entry ->
                    LogEntryRow(entry)
                    if (index != entries.lastIndex) {
                        Spacer(modifier = Modifier.height(10.dp))
                    }
                }
            }
        }
    }
}

@Composable
private fun LogEntryRow(entry: LogEntry) {
    Row(verticalAlignment = Alignment.CenterVertically) {
        Text(text = logIcon(entry.type), fontSize = 18.sp)
        Spacer(modifier = Modifier.width(10.dp))
        Column {
            Text(text = logSummary(entry), color = Color.White, fontSize = 14.sp)
            Text(
                text = timeFormatter.format(entry.timestamp),
                color = Color.White.copy(alpha = 0.6f),
                fontSize = 11.sp,
            )
        }
    }
}

@Composable
private fun StatusCard(label: String, detail: String?, color: Color) {
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .background(color, RoundedCornerShape(24.dp))
            .padding(28.dp),
        contentAlignment = Alignment.Center,
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text(text = label, color = Color(0xFF0D0D14), fontSize = 28.sp, fontWeight = FontWeight.Black)
            if (detail != null) {
                Spacer(modifier = Modifier.height(8.dp))
                Text(text = detail, color = Color(0xFF0D0D14), fontSize = 14.sp)
            }
        }
    }
}

@Composable
private fun InstructionsCard() {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .background(Color(0x0AFFFFFF), RoundedCornerShape(24.dp))
            .padding(20.dp),
    ) {
        Text("Passo 1 — Emparelhar", color = Color.White, fontWeight = FontWeight.Bold)
        Text(
            "No Pokémon GO: Menu Poké Ball → Definições → Dispositivos Ligados → " +
                "Nintendo Switch → Conectar à Nintendo Switch.",
            color = Color.White.copy(alpha = 0.85f),
        )
        Text("Clique no botão 'Conectar Switch' acima para iniciar a busca.", color = Color.White.copy(alpha = 0.85f))
        Spacer(modifier = Modifier.height(16.dp))
        Text("Passo 2 — Enviar Postal (Coin Bag)", color = Color.White, fontWeight = FontWeight.Bold)
        Text(
            "No Pokémon GO: Livro de Postais → Escolha um postal → ENVIAR PARA A NINTENDO SWITCH.",
            color = Color.White.copy(alpha = 0.85f),
        )
        Text("Clique em 'Conectar Switch' novamente para receber o postal e ganhar o Coin Bag.", color = Color.White.copy(alpha = 0.85f))
    }
}
