using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeGraph.Models {
    public class UserData {
        public QuizData data;
        public List<Question> questions;
        public UserData (QuizData _data) {
            data = _data;
        }
    }
}